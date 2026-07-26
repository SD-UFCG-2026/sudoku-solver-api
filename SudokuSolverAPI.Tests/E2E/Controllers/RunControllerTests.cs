using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SudokuSolverAPI.BackgroundServices;
using SudokuSolverAPI.Channels;
using SudokuSolverAPI.Controllers;
using SudokuSolverAPI.DTOs;
using SudokuSolverAPI.Interfaces;
using SudokuSolverAPI.Services;

namespace SudokuSolverAPI.Tests.E2E.Controllers;

public class RunControllerE2ETests : IDisposable
{
    private readonly TestServer _server;
    private readonly HttpClient _client;
    private readonly IBoardPersisterService _persister;
    private readonly Signature _dummySignature = new("Gabael", "9ef9620b6f3f508a7ace91dc8f6ba9e375aecd4360fedeaf04ba561ae27fc51c");
    public RunControllerE2ETests()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"VALIDATION_WORKER_COUNT", "2"},
            {"VALIDATION_CHANNEL_CAPACITY", "10"},
            {"PROCESSING_CHANNEL_CAPACITY", "10"}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<IConfiguration>(configuration);
                
                services.AddControllers().AddApplicationPart(typeof(RunController).Assembly);

                services.AddSingleton<ValidationChannel>();
                services.AddSingleton<ProcessingChannel>();

                services.AddSingleton<IBoardPersisterService, BoardPersisterService>();
                services.AddSingleton<IBoardValidatorService, BoardValidatorService>();
                services.AddSingleton<IBoardProcesserService, BoardProcesserService>();

                services.AddHostedService<ValidationBackgroundService>();
                services.AddHostedService<ProcessingBackgroundService>();
                
                services.AddLogging(l => l.ClearProviders());
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            });

        _server = new TestServer(builder);
        _client = _server.CreateClient();

        _persister = _server.Services.GetRequiredService<IBoardPersisterService>();
    }

    public void Dispose()
    {
        _client.Dispose();
        _server.Dispose();
    }

    [Fact]
    public async Task Post_WhenBoardIsValid_ShouldReturnAccepted_AndProcessEndToEnd()
    {
        int runId = 1;
        
        int[,] rootBoardData = {
            { 1, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };
        var rootNode = new BoardNode(new Board(rootBoardData, _dummySignature!));
        var initialRun = new BoardRun(runId, rootNode) { Id = runId };
        await _persister.SaveRun(initialRun);

        int[,] evolutionBoardData = {
            { 1, 2, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };
        var payload = new BoardDto(evolutionBoardData, _dummySignature!);

        var postResponse = await _client.PostAsJsonAsync($"/api/sudoku/{runId}", payload);

        Assert.Equal(HttpStatusCode.Accepted, postResponse.StatusCode);

        await Task.Delay(300);

        var getResponse = await _client.GetAsync($"/api/sudoku/{runId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var runDto = await getResponse.Content.ReadFromJsonAsync<RunDto>();
        
        Assert.NotNull(runDto);
        
        var runNoBanco = await _persister.Get(runId);
        Assert.Single(runNoBanco.Root.Nodes);
    }

    [Fact]
    public async Task Get_WhenIdDoesNotExist_ShouldReturnNotFound()
    {
        var getResponse = await _client.GetAsync("/api/sudoku/999");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Post_WhenChannelCapacityIsExceeded_ShouldReturnTooManyRequests()
    {
        var limitedConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> {
                {"VALIDATION_CHANNEL_CAPACITY", "1"} // Apenas 1 item permitido!
            })
            .Build();

        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<IConfiguration>(limitedConfig);
                services.AddControllers().AddApplicationPart(typeof(RunController).Assembly);
                services.AddSingleton<ValidationChannel>();
                services.AddSingleton<IBoardPersisterService, BoardPersisterService>();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(e => e.MapControllers());
            });

        using var testServer = new TestServer(builder);
        using var client = testServer.CreateClient();

        int[,] boardData = {
            { 1, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }
        };
        var payload = new BoardDto(boardData, _dummySignature!);

        var response1 = await client.PostAsJsonAsync("/api/sudoku/1", payload);
        Assert.Equal(HttpStatusCode.Accepted, response1.StatusCode);

        var response2 = await client.PostAsJsonAsync("/api/sudoku/1", payload);
        Assert.Equal(HttpStatusCode.TooManyRequests, response2.StatusCode);
    }
}