using SudokuSolverAPI.DTOs;

namespace SudokuSolverAPI;

public class BoardNode(Board value)
{
    public Board Value { get; set; } = value;
    public List<BoardNode> Nodes { get; set; } = [];

    public NodeDto toDTO()
    {
        return new NodeDto(Value.toDTO(), [.. Nodes.Select(n => n.toDTO())]);
    }
}