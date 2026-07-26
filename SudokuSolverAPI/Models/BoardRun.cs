using MongoDB.Bson.Serialization.Attributes;
using SudokuSolverAPI.DTOs;

namespace SudokuSolverAPI;

public class BoardRun(int id, BoardNode root)
{
    [BsonId]
    public int Id { get; set; } = id;
    public BoardNode Root { get; set; } = root;

    public ISet<string> Boards { get; set; } = new HashSet<string>();

    public bool IsResolved { get; set; } = false;
    public BoardNode? Final { get; set; } = null;

    public RunDto toDTO()
    {
        return new RunDto(Root.toDTO(), IsResolved, Final?.toDTO());
    }
}