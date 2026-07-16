namespace SudokuSolverAPI;

public class BoardRun(BoardNode root)
{
    public int Id { get; set; }
    public BoardNode Root { get; }

    public ISet<string> Boards { get; set; } = new HashSet<string>();

    public bool IsResolved { get; set; } = false;
    public BoardNode? Final { get; set; } = null;

}