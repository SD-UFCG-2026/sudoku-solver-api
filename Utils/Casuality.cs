namespace SudokuSolverAPI.Utils;

public static class Casuality
{
    public static bool IsCasualTo(Board root, Board node)
    {
        var rowsRoot = root.Rows;
        var rowsNode = node.Rows;

        for (int i = 0; i < rowsRoot.GetLength(0); i++)
        for (int j = 0; j < rowsNode.GetLength(1); j++)
            if (rowsRoot[i, j] != 0
                && rowsRoot[i, j] != rowsNode[i, j])
                return false;
        return true;
    }
}