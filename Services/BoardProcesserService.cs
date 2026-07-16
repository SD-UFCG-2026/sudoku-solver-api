using SudokuSolverAPI.Interfaces;
using SudokuSolverAPI.Utils;

namespace SudokuSolverAPI.Services;

public class BoardProcesserService : IBoardProcesserService
{
    public BoardRun Process(BoardRun run, BoardNode node)
    {
        ProcessTree(run.Root, node);
        FinishTree(run, node);
        return run;
    }

    private void FinishTree(BoardRun run, BoardNode node)
    {
        if (IsBoardResolved(node.Value))
        {
            run.IsResolved = true;
            run.Final = node;
        }
    }

    private void ProcessTree(BoardNode root, BoardNode node)
    {
        var bestParent = FindDeepestParent(root, node);
        if (bestParent != null)
        {
            bestParent.Nodes.Add(node);
            node.Root = bestParent;

            for (int i = bestParent.Nodes.Count - 2; i >= 0 ; i--)
            {
                var sibling = bestParent.Nodes[i];
                if (Casuality.IsCasualTo(node.Value, sibling.Value))
                {
                    bestParent.Nodes.RemoveAt(i);
                    node.Nodes.Add(sibling);
                    sibling.Root = node;
                }
            }
        }
    }

    private BoardNode? FindDeepestParent(BoardNode current, BoardNode newNode)
    {
        if (!Casuality.IsCasualTo(current.Value, newNode.Value))
            return null;

        foreach (var child in current.Nodes)
        {
            var deeperParent = FindDeepestParent(child, newNode);
            if (deeperParent != null)
                return deeperParent;
        }

        return current;
    }

    private bool IsBoardResolved(Board board)
    {
        var rows = board.Rows;
        for (var i = 0; i < rows.GetLength(0); i++)
            for (var j = 0; j < rows.GetLength(1); j++)
                if (rows[i, j] == 0)
                    return false;
        return true;
    }
}