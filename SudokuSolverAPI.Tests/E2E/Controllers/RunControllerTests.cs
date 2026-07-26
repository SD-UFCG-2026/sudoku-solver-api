using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SudokuSolverAPI.DTOs;
using SudokuSolverAPI.Utils;
using Xunit;

namespace SudokuSolverAPI.Tests.E2E.Controllers;

public class RunControllerE2ETests : E2ETests
{
    private readonly Signature _dummySignature = new("Gabael", "9ef9620b6f3f508a7ace91dc8f6ba9e375aecd4360fedeaf04ba561ae27fc51c");

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new MultidimensionalArrayConverter() }
    };

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
        var rootNode = new BoardNode(new Board(rootBoardData, _dummySignature));
        var initialRun = new BoardRun(runId, rootNode) { Id = runId };

        await Persister.SaveRun(initialRun);

        int[,] evolutionBoardData = {
            { 1, 2, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };
        var payload = new BoardDto(evolutionBoardData, _dummySignature);

        var postResponse = await Client.PostAsJsonAsync($"/api/sudoku/{runId}", payload, _jsonOptions);
        Assert.Equal(HttpStatusCode.Accepted, postResponse.StatusCode);

        await Task.Delay(300);

        var getResponse = await Client.GetAsync($"/api/sudoku/{runId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var runDto = await getResponse.Content.ReadFromJsonAsync<RunDto>(_jsonOptions);

        Assert.NotNull(runDto);
        Assert.NotNull(runDto.Root);
        Assert.False(runDto.IsFinished);
        Assert.Single(runDto.Root.Child);

        var childNode = runDto.Root.Child[0];
        Assert.Equal(1, childNode.Value.Board[0, 0]);
        Assert.Equal(2, childNode.Value.Board[0, 1]);
    }

    [Fact]
    public async Task Get_WhenIdDoesNotExist_ShouldReturnNotFound()
    {
        var getResponse = await Client.GetAsync("/api/sudoku/999");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Post_WhenChannelCapacityIsExceeded_ShouldReturnTooManyRequests()
    {
        var limitedConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                {"MongoCollectionName", "test_runs"},
                {"VALIDATION_CHANNEL_CAPACITY", "1"}
            })
            .Build();

        using var testServer = CreateTestServer(limitedConfig, Database, includeBackgroundServices: false);
        using var client = testServer.CreateClient();

        int[,] boardData = {
            { 1, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }
        };
        var payload = new BoardDto(boardData, _dummySignature);

        var response1 = await client.PostAsJsonAsync("/api/sudoku/1", payload, _jsonOptions);
        Assert.Equal(HttpStatusCode.Accepted, response1.StatusCode);

        var response2 = await client.PostAsJsonAsync("/api/sudoku/1", payload, _jsonOptions);
        Assert.Equal(HttpStatusCode.TooManyRequests, response2.StatusCode);
    }
}