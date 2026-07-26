using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace SudokuSolverAPI.Utils;

public static class MongoConfig
{
    public static void RegisterCustomSerializers()
    {
        try
        {
            BsonSerializer.RegisterSerializer(typeof(int[,]), new MultidimensionalArrayBsonSerializer());
        }
        catch (BsonSerializationException ignore)
        {
        }
    }
}