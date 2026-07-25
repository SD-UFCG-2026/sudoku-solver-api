using System.Collections.Concurrent;
using SudokuSolverAPI.Interfaces;
using SudokuSolverAPI.Utils;

namespace SudokuSolverAPI.Services;

public class BoardValidatorService: IBoardValidatorService
{
    private readonly ConcurrentDictionary<string, byte> _processedHashes = new();

    public bool IsValid(BoardRun run, Board board)
    {
        string stringValue = board.SudokuVisualize;

        if (!_processedHashes.TryAdd(stringValue, 0)) return false;

        run.Boards.Add(stringValue);

        return IsValid(board.Rows)
            && IsValid(board.Cols)
            && IsValid(board.Qs)
            && Casuality.IsCasualTo(run.Root.Value, board);
    }

    private bool IsValid(int[,] lines)
    {
        for (int i = 0; i < lines.GetLength(0); i++)
        {
            var numbers = new bool[lines.GetLength(1)];
            for (int k = 0; k < numbers.GetLength(1); k++)
                numbers[k] = false;
            for (int j = 0; j < lines.GetLength(1); j++)
            {
                var n = lines[i, j] - 1;
                if (n == -1) continue;
                if (numbers[n]) return false;
                numbers[n] = true;
            }
        }

        return true;
    }
}