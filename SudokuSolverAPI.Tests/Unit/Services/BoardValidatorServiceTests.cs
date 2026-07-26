using SudokuSolverAPI.Services;
using SudokuSolverAPI.Interfaces;

namespace SudokuSolverAPI.Tests.Services;

public class BoardValidatorServiceTests
{
    private readonly IBoardValidatorService _validatorService = new BoardValidatorService();
    private readonly Signature? _dummySignature = null;

    [Fact]
    public void IsValid_WhenBoardIsValidAndEvolvesFromRoot_ShouldReturnTrue()
    {
        int[,] rootBoard = {
            { 1, 2, 0, 0 },
            { 0, 0, 1, 2 },
            { 2, 1, 0, 0 },
            { 0, 0, 2, 1 }
        };

        int[,] validEvolutionBoard = {
            { 1, 2, 3, 4 },
            { 3, 4, 1, 2 },
            { 2, 1, 4, 3 },
            { 4, 3, 2, 1 }
        };

        var rootNode = new BoardNode(new Board(rootBoard, _dummySignature!));
        var boardRun = new BoardRun(0, rootNode);
        var boardToTest = new Board(validEvolutionBoard, _dummySignature!);

        bool isValid = _validatorService.IsValid(boardRun, boardToTest);

        Assert.True(isValid);
        Assert.Contains(boardToTest.SudokuVisualize, boardRun.Boards);
    }

    [Fact]
    public void IsValid_WhenBoardHasDuplicateInRow_ShouldReturnFalse()
    {
        int[,] rootBoard = {
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        int[,] invalidRowBoard = {
            { 1, 1, 3, 4 },
            { 3, 4, 1, 2 },
            { 2, 1, 4, 3 },
            { 4, 3, 2, 1 }
        };

        var rootNode = new BoardNode(new Board(rootBoard, _dummySignature!));
        var boardRun = new BoardRun(0, rootNode);
        var boardToTest = new Board(invalidRowBoard, _dummySignature!);

        bool isValid = _validatorService.IsValid(boardRun, boardToTest);

        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_WhenBoardViolatesCasuality_ShouldReturnFalse()
    {
        int[,] rootBoard = {
            { 1, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        int[,] conflictingBoard = {
            { 2, 3, 1, 4 },
            { 3, 4, 2, 1 },
            { 1, 2, 4, 3 },
            { 4, 1, 3, 2 }
        };

        var rootNode = new BoardNode(new Board(rootBoard, _dummySignature!));
        var boardRun = new BoardRun(0, rootNode);
        var boardToTest = new Board(conflictingBoard, _dummySignature!);

        bool isValid = _validatorService.IsValid(boardRun, boardToTest);

        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_WhenBoardIsSubmittedTwice_ShouldReturnFalseOnSecondTime()
    {
        int[,] rootBoard = {
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        int[,] validBoard = {
            { 1, 2, 3, 4 },
            { 3, 4, 1, 2 },
            { 2, 1, 4, 3 },
            { 4, 3, 2, 1 }
        };

        var rootNode = new BoardNode(new Board(rootBoard, _dummySignature!));
        var boardRun = new BoardRun(0, rootNode);
        var boardToTest1 = new Board(validBoard, _dummySignature!);
        var boardToTest2 = new Board(validBoard, _dummySignature!);

        bool firstSubmission = _validatorService.IsValid(boardRun, boardToTest1);
        bool secondSubmission = _validatorService.IsValid(boardRun, boardToTest2);

        Assert.True(firstSubmission);
        Assert.False(secondSubmission);
    }
}