using System.Text.Json.Serialization;
using SudokuSolverAPI.Utils;

namespace SudokuSolverAPI.DTOs;

public record BoardDto (
    [property: JsonConverter(typeof(MultidimensionalArrayConverter))] int[,] Board,
    Signature Signature)
{
    public Board ToEntity()
    {
        return new Board(Board, Signature);
    }
}

/*
 * {
 *      board: [[1,2,3,4],[3,4,2,1],[0,0,0,0],[0,0,0,0]],
 *      signature: {
 *          identifier: "Gabael",
 *          "key":      "9ef9620b6f3f508a7ace91dc8f6ba9e375aecd4360fedeaf04ba561ae27fc51c"
 *      }
 * }
 */