using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using SudokuSolverAPI.BackgroundServices;
using SudokuSolverAPI.Channels;
using SudokuSolverAPI.Services;

namespace SudokuSolverAPI.Tests.Integration.BackgroundServices;

public class ProcessingBackgroundServiceIntegrationTests : IAsyncLifetime
{
    private readonly Signature? _dummySignature = null;

    private readonly IConfiguration _config;
    private readonly ProcessingChannel _processingChannel;
    private readonly BoardPersisterService _persisterService;
    private readonly BoardProcesserService _processerService;
    private readonly ProcessingBackgroundService _backgroundService;
    private readonly CancellationTokenSource _backgroundCts;

    private readonly BoardRun _defaultRun;

    public ProcessingBackgroundServiceIntegrationTests()
    {
        _config = BuildTestConfiguration();

        _processingChannel = new ProcessingChannel(_config);

        _persisterService = new BoardPersisterService();
        _processerService = new BoardProcesserService();

        _backgroundService = new ProcessingBackgroundService(
            _processingChannel,
            _processerService,
            _persisterService,
            NullLogger<ProcessingBackgroundService>.Instance
        );

        _backgroundCts = new CancellationTokenSource();

        int[,] rootBoardData = {
            { 1, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };
        var rootNode = new BoardNode(new Board(rootBoardData, _dummySignature!));

        _defaultRun = new BoardRun(0, rootNode) { Id = 1 };
    }

    public async Task InitializeAsync()
    {
        await _persisterService.SaveRun(_defaultRun);
        await _backgroundService.StartAsync(_backgroundCts.Token);
    }

    public async Task DisposeAsync()
    {
        _backgroundCts.Cancel();
        await _backgroundService.StopAsync(CancellationToken.None);
    }

    private IConfiguration BuildTestConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"PROCESSING_CHANNEL_CAPACITY", "100"}
        };
        return new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
    }


    [Fact]
    public async Task ProcessData_ShouldAttachChildNodeToRoot()
    {
        int[,] nodeData = {
            { 1, 2, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };
        var node = new BoardNode(new Board(nodeData, _dummySignature!));

        await _processingChannel.Writer.WriteAsync(new ProcessingData(_defaultRun.Id, node));
        await WaitProcess();

        var updatedRun = await _persisterService.Get(_defaultRun.Id);

        Assert.Single(updatedRun.Root.Nodes);
        Assert.Equal(node, updatedRun.Root.Nodes[0]);
        Assert.False(updatedRun.IsResolved);
    }

    [Fact]
    public async Task ProcessData_ShouldReorderTree_WhenIntermediateNodeArrivesLate()
    {
        int[,] nodeAData = {
            { 1, 2, 3, 4 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };
        var nodeA = new BoardNode(new Board(nodeAData, _dummySignature!));

        int[,] nodeBData = {
            { 1, 2, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };
        var nodeB = new BoardNode(new Board(nodeBData, _dummySignature!));

        await _processingChannel.Writer.WriteAsync(new ProcessingData(_defaultRun.Id, nodeA));
        await WaitProcess();

        await _processingChannel.Writer.WriteAsync(new ProcessingData(_defaultRun.Id, nodeB));
        await WaitProcess();

        var updatedRun = await _persisterService.Get(_defaultRun.Id);

        Assert.Single(updatedRun.Root.Nodes);
        Assert.Equal(nodeB, updatedRun.Root.Nodes[0]);

        Assert.Single(nodeB.Nodes);
        Assert.Equal(nodeA, nodeB.Nodes[0]);
    }

    [Fact]
    public async Task ProcessData_ShouldMarkRunAsResolved_WhenBoardIsComplete()
    {
        int[,] resolvedData = {
            { 1, 2, 3, 4 },
            { 3, 4, 1, 2 },
            { 2, 1, 4, 3 },
            { 4, 3, 2, 1 }
        };
        var resolvedNode = new BoardNode(new Board(resolvedData, _dummySignature!));

        await _processingChannel.Writer.WriteAsync(new ProcessingData(_defaultRun.Id, resolvedNode));
        await WaitProcess();

        var updatedRun = await _persisterService.Get(_defaultRun.Id);

        Assert.True(updatedRun.IsResolved);
        Assert.NotNull(updatedRun.Final);
        Assert.Equal(resolvedNode, updatedRun.Final);
    }

    [Fact]
    public async Task ProcessData_UnknownRunId_ShouldNotCrashAndContinueProcessing()
    {
        int[,] validData = {
            { 1, 2, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };
        var node = new BoardNode(new Board(validData, _dummySignature!));

        await _processingChannel.Writer.WriteAsync(new ProcessingData(999, node));
        await WaitProcess();

        await _processingChannel.Writer.WriteAsync(new ProcessingData(_defaultRun.Id, node));
        await WaitProcess();

        var updatedRun = await _persisterService.Get(_defaultRun.Id);

        Assert.Single(updatedRun.Root.Nodes);
    }

    private async Task WaitProcess()
    {
        await Task.Delay(200);
    }
}