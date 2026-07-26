using SudokuSolverAPI.Utils;

namespace SudokuSolverAPI.Tests.Unit.Utils;

public class CasualityTests
{
    private readonly Signature? _dummySignature = null;

    [Fact]
    public void IsCasualTo_WhenNodeIsEvolutionOfRoot_ShouldReturnTrue()
    {
        int[,] rootBoard = new int[,]
        {
            { 5, 3, 0 },
            { 6, 0, 0 },
            { 0, 9, 8 }
        };

        int[,] nodeBoard = new int[,]
        {
            { 5, 3, 1 },
            { 6, 4, 0 },
            { 2, 9, 8 }
        };

        var root = new Board(rootBoard, _dummySignature!);
        var node = new Board(nodeBoard, _dummySignature!);

        bool result = Casuality.IsCasualTo(root, node);

        Assert.True(result);
    }

    [Fact]
    public void IsCasualTo_WhenNodeHasConflictWithRoot_ShouldReturnFalse()
    {
        int[,] rootBoard = new int[,]
        {
            { 5, 3, 0 },
            { 6, 0, 0 },
            { 0, 9, 8 }
        };

        int[,] nodeBoard = new int[,]
        {
            { 5, 4, 1 },
            { 6, 4, 0 },
            { 2, 9, 8 }
        };

        var root = new Board(rootBoard, _dummySignature!);
        var node = new Board(nodeBoard, _dummySignature!);

        bool result = Casuality.IsCasualTo(root, node);

        Assert.False(result);
    }

    [Fact]
    public void IsCasualTo_WhenRootIsEmpty_ShouldReturnTrue()
    {
        int[,] rootBoard = new int[,]
        {
            { 0, 0 },
            { 0, 0 }
        };

        int[,] nodeBoard = new int[,]
        {
            { 1, 2 },
            { 3, 4 }
        };

        var root = new Board(rootBoard, _dummySignature!);
        var node = new Board(nodeBoard, _dummySignature!);

        bool result = Casuality.IsCasualTo(root, node);

        Assert.True(result);
    }

    [Fact]
    public void IsCasualTo_WhenBoardsAreIdentical_ShouldReturnTrue()
    {
        int[,] boardData = new int[,]
        {
            { 5, 3 },
            { 6, 7 }
        };

        var root = new Board(boardData, _dummySignature!);
        var node = new Board(boardData, _dummySignature!);

        bool result = Casuality.IsCasualTo(root, node);

        Assert.True(result);
    }
}