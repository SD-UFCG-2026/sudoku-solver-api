using SudokuSolverAPI.Interfaces;
using SudokuSolverAPI.Services;

namespace SudokuSolverAPI.Tests.Unit.Services;

public class BoardProcesserServiceTests
{
    private readonly IBoardProcesserService _processorService = new BoardProcesserService();
    private readonly Signature? _dummySignature = null;

    [Fact]
    public void Process_ShouldAttachNodeToDeepestParent()
    {

        int[,] rootData = {
            { 1, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        int[,] nodeAData = {
            { 1, 2, 0, 0 },
            { 3, 4, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        int[,] nodeBData = {
            { 1, 2, 3, 0 },
            { 3, 4, 1, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        var rootNode = new BoardNode(new Board(rootData, _dummySignature!));
        var run = new BoardRun(0, rootNode);

        var nodeA = new BoardNode(new Board(nodeAData, _dummySignature!));
        var nodeB = new BoardNode(new Board(nodeBData, _dummySignature!));

        _processorService.Process(run, nodeA);
        _processorService.Process(run, nodeB);

        Assert.Single(run.Root.Nodes);
        Assert.Equal(nodeA, run.Root.Nodes[0]);

        Assert.Single(nodeA.Nodes);
        Assert.Equal(nodeB, nodeA.Nodes[0]);
    }

    [Fact]
    public void Process_ShouldReorderTree_WhenNewNodeIsPredecessorOfExistingSibling()
    {
        var processor = new BoardProcesserService();

        int[,] rootData = {
            { 1, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        int[,] nodeAData = {
            { 1, 2, 3, 4 },
            { 3, 4, 1, 2 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        int[,] nodeBData = {
            { 1, 2, 0, 0 },
            { 3, 4, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        var rootNode = new BoardNode(new Board(rootData, _dummySignature!));
        var run = new BoardRun(0, rootNode);

        var nodeA = new BoardNode(new Board(nodeAData, _dummySignature!));
        var nodeB = new BoardNode(new Board(nodeBData, _dummySignature!));

        _processorService.Process(run, nodeA);
        _processorService.Process(run, nodeB);

        Assert.Single(run.Root.Nodes);
        Assert.Equal(nodeB, run.Root.Nodes[0]);

        Assert.Single(nodeB.Nodes);
        Assert.Equal(nodeA, nodeB.Nodes[0]);
    }

    [Fact]
    public void Process_ShouldMarkRunAsResolved_WhenBoardIsCompletelyFilled()
    {
        var processor = new BoardProcesserService();

        int[,] rootData = {
            { 1, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        int[,] resolvedData = {
            { 1, 2, 3, 4 },
            { 3, 4, 1, 2 },
            { 2, 1, 4, 3 },
            { 4, 3, 2, 1 }
        };

        var rootNode = new BoardNode(new Board(rootData, _dummySignature!));
        var run = new BoardRun(0, rootNode);

        var resolvedNode = new BoardNode(new Board(resolvedData, _dummySignature!));

        _processorService.Process(run, resolvedNode);

        Assert.True(run.IsResolved);
        Assert.NotNull(run.Final);
        Assert.Equal(resolvedNode, run.Final);
    }
}