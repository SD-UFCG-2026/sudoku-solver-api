using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SudokuSolverAPI.BackgroundServices;
using SudokuSolverAPI.Channels;
using SudokuSolverAPI.Controllers;
using SudokuSolverAPI.Interfaces;
using SudokuSolverAPI.Services;
using SudokuSolverAPI.Utils;
using Testcontainers.MongoDb;
using Xunit;

namespace SudokuSolverAPI.Tests.E2E;

public abstract class E2ETests : IAsyncLifetime
{
    protected readonly MongoDbContainer MongoDbContainer;
    protected IMongoDatabase Database = null!;
    protected IConfiguration Configuration = null!;

    protected TestServer Server = null!;
    protected HttpClient Client = null!;
    protected IBoardPersisterService Persister = null!;

    protected E2ETests()
    {
        MongoDbContainer = new MongoDbBuilder()
            .WithImage("mongo:latest")
            .Build();
    }

    protected virtual Dictionary<string, string?> GetCustomConfiguration() => new()
    {
        {"VALIDATION_WORKER_COUNT", "2"},
        {"VALIDATION_CHANNEL_CAPACITY", "10"},
        {"PROCESSING_CHANNEL_CAPACITY", "10"}
    };

    public virtual async Task InitializeAsync()
    {
        MongoConfig.RegisterCustomSerializers();

        await MongoDbContainer.StartAsync();

        var mongoClient = new MongoClient(MongoDbContainer.GetConnectionString());
        Database = mongoClient.GetDatabase("sudoku_e2e_db");

        var settings = new Dictionary<string, string?>
        {
            {"MongoCollectionName", "test_runs"}
        };

        foreach (var custom in GetCustomConfiguration())
        {
            settings[custom.Key] = custom.Value;
        }

        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        Server = CreateTestServer(Configuration, Database);
        Client = Server.CreateClient();
        Persister = Server.Services.GetRequiredService<IBoardPersisterService>();
    }

    public virtual async Task DisposeAsync()
    {
        Client?.Dispose();
        Server?.Dispose();
        await MongoDbContainer.DisposeAsync().AsTask();
    }

    protected TestServer CreateTestServer(IConfiguration config, IMongoDatabase database, bool includeBackgroundServices = true)
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(config);
                services.AddSingleton(database);

                services.AddControllers()
                    .AddApplicationPart(typeof(RunController).Assembly)
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                        options.JsonSerializerOptions.Converters.Add(new MultidimensionalArrayConverter());
                    });

                services.AddSingleton<ValidationChannel>();
                services.AddSingleton<ProcessingChannel>();

                services.AddSingleton<IBoardPersisterService, BoardPersisterService>();
                services.AddSingleton<IBoardValidatorService, BoardValidatorService>();
                services.AddSingleton<IBoardProcesserService, BoardProcesserService>();

                if (includeBackgroundServices)
                {
                    services.AddHostedService<ValidationBackgroundService>();
                    services.AddHostedService<ProcessingBackgroundService>();
                }

                services.AddLogging(l => l.ClearProviders());
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            });

        return new TestServer(builder);
    }
}