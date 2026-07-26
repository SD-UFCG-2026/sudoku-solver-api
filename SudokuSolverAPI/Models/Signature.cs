namespace SudokuSolverAPI;

public class Signature(string identifier, string key)
{
    public string Identifier { get; } = identifier;
    public string Key { get; } = key;
}