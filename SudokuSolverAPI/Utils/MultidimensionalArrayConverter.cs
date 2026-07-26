using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SudokuSolverAPI.Utils;

public class MultidimensionalArrayConverter : JsonConverter<int[,]>
{
    public override int[,]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        int rows = root.GetArrayLength();
        if (rows == 0) return new int[0, 0];

        int cols = root[0].GetArrayLength();
        var result = new int[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            var row = root[i];
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = row[j].GetInt32();
            }
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, int[,] value, JsonSerializerOptions options)
    {
        int rows = value.GetLength(0);
        int cols = value.GetLength(1);

        writer.WriteStartArray();
        for (int i = 0; i < rows; i++)
        {
            writer.WriteStartArray();
            for (int j = 0; j < cols; j++)
            {
                writer.WriteNumberValue(value[i, j]);
            }
            writer.WriteEndArray();
        }
        writer.WriteEndArray();
    }
}