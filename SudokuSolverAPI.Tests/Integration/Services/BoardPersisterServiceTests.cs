using MongoDB.Driver;
using SudokuSolverAPI.Services;

namespace SudokuSolverAPI.Tests.Integration.Services;

public class BoardPersisterServiceTests : MongoDbIntegrationTestBase
{

    private int[,] _dummyData = {
        { 1, 0, 0, 0 },
        { 0, 0, 0, 0 },
        { 0, 0, 0, 0 },
        { 0, 0, 0, 0 }
    };

    private readonly Signature _dummySignature = new("Gabael", "9ef9620b6f3f508a7ace91dc8f6ba9e375aecd4360fedeaf04ba561ae27fc51c");


    private BoardPersisterService CreateService()
    {
        return new BoardPersisterService(Database, Configuration);
    }

    private IMongoCollection<BoardRun> GetCollection()
    {
        return Database.GetCollection<BoardRun>("test_runs");
    }

    [Fact]
    public async Task SaveRun_ShouldInsertIntoDatabase_And_CacheInDictionary()
    {
        var service = CreateService();
        var run = new BoardRun(0, new BoardNode(new Board(_dummyData, _dummySignature)));

        await service.SaveRun(run);

        var savedInDb = await GetCollection().Find(r => r.Id == 0).FirstOrDefaultAsync();

        Assert.NotNull(savedInDb);
        Assert.Equal(0, savedInDb.Id);
    }

    [Fact]
    public async Task Get_ShouldReturnFromCache_WhenAlreadySaved()
    {
        var service = CreateService();
        var run = new BoardRun(2, new BoardNode(new Board(_dummyData, _dummySignature)));
        await service.SaveRun(run);

        await GetCollection().DeleteOneAsync(r => r.Id == 2);

        var result = await service.Get(2);

        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
    }

    [Fact]
    public async Task Get_ShouldReturnFromDatabase_WhenNotCached()
    {
        var run = new BoardRun(3, new BoardNode(new Board(_dummyData, _dummySignature)));
        await GetCollection().InsertOneAsync(run);

        var service = CreateService();

        var result = await service.Get(3);

        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
    }


    [Fact]
    public async Task Get_ShouldDeserializeMultidimensionalArray_AndNotThrowFormatException()
    {
        var rawBson = MongoDB.Bson.BsonDocument.Parse("""
                                                      {
                                                          "_id": 99,
                                                          "Root": {
                                                              "Value": {
                                                                  "SudokuBoard": [[1, 2], [3, 4]],
                                                                  "Signature": { "Identifier": "Test", "key": "abc" }
                                                              },
                                                              "Nodes": []
                                                          },
                                                          "IsResolved": false
                                                      }
                                                      """);

        var rawCollection = Database.GetCollection<MongoDB.Bson.BsonDocument>("test_runs");
        await rawCollection.InsertOneAsync(rawBson);

        var service = CreateService();

        var result = await service.Get(99);

        Assert.NotNull(result);
        Assert.NotNull(result.Root.Value.SudokuBoard);
        Assert.Equal(1, result.Root.Value.SudokuBoard[0, 0]);
        Assert.Equal(4, result.Root.Value.SudokuBoard[1, 1]);
    }

    [Fact]
    public async Task Get_ShouldThrowKeyNotFoundException_WhenIdDoesNotExist()
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.Get(999));
        Assert.Equal("Run with id: 999, not found", exception.Message);
    }
}