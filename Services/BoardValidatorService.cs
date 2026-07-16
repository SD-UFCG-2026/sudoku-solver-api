using SudokuSolverAPI.Interfaces;
using SudokuSolverAPI.Utils;

namespace SudokuSolverAPI.Services;

public class BoardValidatorService: IBoardValidatorService
{
    public bool IsValid(BoardRun run, Board board)
    {
        string stringValue = board.SudokuVisualize;

        if (!run.Boards.Add(stringValue)) return false;

        if (!IsValid(board.Rows)) return false;
        if (!IsValid(board.Cols)) return false;
        if (!IsValid(board.Qs)) return false;

        if (!Casuality.IsCasualTo(run.Root.Value, board)) return false;

        return true;
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