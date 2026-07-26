using Microsoft.Extensions.Logging.Abstractions;
using SudokuSolverAPI.BackgroundServices;
using SudokuSolverAPI.Channels;
using SudokuSolverAPI.Services;

namespace SudokuSolverAPI.Tests.Integration.BackgroundServices;

public class ProcessingBackgroundServiceIntegrationTests : MongoDbIntegrationTestBase
{
    private readonly Signature? _dummySignature = null;

    private ProcessingChannel _processingChannel = null!;
    private BoardPersisterService _persisterService = null!;
    private BoardProcesserService _processerService = null!;
    private ProcessingBackgroundService _backgroundService = null!;
    private CancellationTokenSource _backgroundCts = null!;

    private readonly BoardRun _defaultRun;

    public ProcessingBackgroundServiceIntegrationTests()
    {
        int[,] rootBoardData = {
            { 1, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };
        var rootNode = new BoardNode(new Board(rootBoardData, _dummySignature!));
        _defaultRun = new BoardRun(0, rootNode) { Id = 1 };
    }

    protected override Dictionary<string, string?> GetCustomConfiguration() => new()
    {
        {"PROCESSING_CHANNEL_CAPACITY", "100"}
    };

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _processingChannel = new ProcessingChannel(Configuration);

        _persisterService = new BoardPersisterService(Database, Configuration);
        _processerService = new BoardProcesserService();

        _backgroundService = new ProcessingBackgroundService(
            _processingChannel,
            _processerService,
            _persisterService,
            NullLogger<ProcessingBackgroundService>.Instance
        );

        _backgroundCts = new CancellationTokenSource();

        await _persisterService.SaveRun(_defaultRun);
        await _backgroundService.StartAsync(_backgroundCts.Token);
    }

    public override async Task DisposeAsync()
    {
        _backgroundCts.Cancel();
        await _backgroundService.StopAsync(CancellationToken.None);

        await base.DisposeAsync(); // Derruba Mongo
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