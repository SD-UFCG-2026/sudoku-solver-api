using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace SudokuSolverAPI.Utils;

public class MultidimensionalArrayBsonSerializer : SerializerBase<int[,]>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, int[,] value)
    {
        if (value == null)
        {
            context.Writer.WriteNull();
            return;
        }

        context.Writer.WriteStartArray();
        int rows = value.GetLength(0);
        int cols = value.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            context.Writer.WriteStartArray();
            for (int j = 0; j < cols; j++)
            {
                context.Writer.WriteInt32(value[i, j]);
            }
            context.Writer.WriteEndArray();
        }
        context.Writer.WriteEndArray();
    }

    public override int[,] Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        if (context.Reader.CurrentBsonType == BsonType.Null)
        {
            context.Reader.ReadNull();
            return null!;
        }

        context.Reader.ReadStartArray();
        var rowsList = new List<List<int>>();

        while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            context.Reader.ReadStartArray();
            var colsList = new List<int>();
            while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
            {
                colsList.Add(context.Reader.ReadInt32());
            }
            context.Reader.ReadEndArray();
            rowsList.Add(colsList);
        }
        context.Reader.ReadEndArray();

        int rows = rowsList.Count;
        int cols = rows > 0 ? rowsList[0].Count : 0;
        int[,] result = new int[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = rowsList[i][j];
            }
        }

        return result;
    }
}