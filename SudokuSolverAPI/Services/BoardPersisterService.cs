using System.Collections.Concurrent;
using SudokuSolverAPI.Interfaces;

namespace SudokuSolverAPI.Services;

public class BoardPersisterService : IBoardPersisterService
{
    private ConcurrentDictionary<int, BoardRun> BoardMap = [];
    public Task<BoardRun> SaveRun(BoardRun run)
    {
        try
        {
            BoardMap.GetOrAdd(run.Id, _ => run);
            return Task.FromResult(run);
        }
        catch (Exception exception)
        {
            return Task.FromException<BoardRun>(exception);
        }
    }

    public Task<BoardRun> Get(int id)
    {
        try
        {
            return Task.FromResult(BoardMap[id]);
        }
        catch (Exception exception)
        {
            return Task.FromException<BoardRun>(exception);
        }
    }
}