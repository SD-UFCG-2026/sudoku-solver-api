namespace SudokuSolverAPI.DTOs;

public record BoardDto (int[,] Board, Signature Signature)
{

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