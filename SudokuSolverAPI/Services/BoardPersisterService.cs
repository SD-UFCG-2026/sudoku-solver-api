using System.Collections.Concurrent;
using MongoDB.Driver;
using SudokuSolverAPI.Interfaces;

namespace SudokuSolverAPI.Services;

public class BoardPersisterService(
    IMongoDatabase database,
    IConfiguration configuration) : IBoardPersisterService
{
    private ConcurrentDictionary<int, BoardRun> BoardMap = [];

    private readonly IMongoCollection<BoardRun> _collection = database.GetCollection<BoardRun>(
        configuration["MongoCollectionName"] ?? "runs");

    public async Task<BoardRun> SaveRun(BoardRun run)
    {
        try
        {
            var filter = Builders<BoardRun>.Filter.Eq(r => r.Id, run.Id);
            var options = new ReplaceOptions { IsUpsert = true };

            await _collection.ReplaceOneAsync(filter, run, options);

            BoardMap[run.Id] = run;

            return run;
        }
        catch (Exception ex)
        {
            throw new Exception("Error on save on MongoDB");
        }
    }

    public async Task<BoardRun> Get(int id)
    {
        if (BoardMap.TryGetValue(id, out var cached))
            return cached;

        try
        {
            var filter = Builders<BoardRun>.Filter.Eq(r => r.Id, id);
            var dbRun = await _collection.Find(filter).FirstOrDefaultAsync();

            if (dbRun == null)
            {
                throw new KeyNotFoundException($"Run with id: {id}, not found");
            }

            BoardMap[id] = dbRun;

            return dbRun;
        }
        catch (Exception exception) when (exception is not KeyNotFoundException)
        {
            throw new Exception("Search on mongo error:", exception);
        }

    }

    public async Task<List<BoardRun>> GetAll()
    {
        try
        {
            var runs = await _collection.Find(_ => true).ToListAsync();

            return runs;
        }
        catch (Exception exception)
        {
            throw new Exception("Search all on mongo error:", exception);
        }
    }
}