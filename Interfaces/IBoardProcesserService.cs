namespace SudokuSolverAPI.Interfaces;

public interface IBoardProcesserService
{
    public BoardRun Process(BoardRun run, BoardNode node);
}