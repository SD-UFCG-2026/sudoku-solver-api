using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Xunit;

namespace SudokuSolverAPI.Tests.Integration;

public abstract class MongoDbIntegrationTestBase : IAsyncLifetime
{
    protected readonly MongoDbContainer MongoDbContainer;
    protected IMongoDatabase Database = null!;
    protected IConfiguration Configuration = null!;

    protected MongoDbIntegrationTestBase()
    {
        MongoDbContainer = new MongoDbBuilder()
            .WithImage("mongo:latest")
            .Build();
    }

    protected virtual Dictionary<string, string?> GetCustomConfiguration()
        => new Dictionary<string, string?>();

    public virtual async Task InitializeAsync()
    {
        await MongoDbContainer.StartAsync();

        var client = new MongoClient(MongoDbContainer.GetConnectionString());
        Database = client.GetDatabase("sudoku_test_db");

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
    }

    public virtual async Task DisposeAsync()
    {
        await MongoDbContainer.DisposeAsync().AsTask();
    }
}