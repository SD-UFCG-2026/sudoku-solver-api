namespace SudokuSolverAPI.Interfaces;

public interface IBoardValidatorService
{
    public bool IsValid(BoardRun run, Board board);
}