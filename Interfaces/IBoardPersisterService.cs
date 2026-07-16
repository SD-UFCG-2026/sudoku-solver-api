namespace SudokuSolverAPI.Interfaces;

public interface IBoardPersisterService
{
    public BoardRun SaveRun(BoardRun run);

    public BoardRun Get(int id);

    public BoardRun Finish(int id, BoardNode finalNode);

    public BoardRun Delete(int id);
}