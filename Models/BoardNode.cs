namespace SudokuSolverAPI;

public class BoardNode(Board value, BoardNode? root)
{
    public Board Value { get; }
    public BoardNode? root { get; set; } = null;
    public List<BoardNode> Nodes { get; } = new List<BoardNode>();
}