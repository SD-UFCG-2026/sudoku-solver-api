namespace SudokuSolverAPI.Interfaces;

public interface IBoardPersisterService
{
    public Task<BoardRun> SaveRun(BoardRun run);

    public Task<BoardRun> Get(int id);

    public Task<List<BoardRun>> GetAll();
}