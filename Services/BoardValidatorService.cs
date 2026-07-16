using SudokuSolverAPI.Interfaces;

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

        if (!IsValid(run.Root.Value, board)) return false;

        return true;
    }

    private bool IsValid(int[,] lines)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var numbers = new bool[lines.Length];
            for (int k = 0; k < numbers.Length; k++)
                numbers[k] = false;
            for (int j = 0; j < lines.Length; j++)
            {
                var n = lines[i, j] - 1;
                if (n == -1) continue;
                if (numbers[n]) return false;
                numbers[n] = true;
            }
        }

        return true;
    }

    private bool IsValid(Board root, Board node)
    {
        var rowsRoot = root.Rows;
        var rowsNode = node.Rows;

        for (int i = 0; i < rowsRoot.Length; i++)
            for (int j = 0; j < rowsNode.Length; j++)
                if (rowsRoot[i, j] != 0
                    && rowsRoot[i, j] != rowsNode[i, j])
                    return false;
        return true;
    }
}