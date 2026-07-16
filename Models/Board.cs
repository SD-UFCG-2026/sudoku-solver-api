namespace SudokuSolverAPI;

public class Board(int[,] sudokuBoard, Signature signature)
{
    public int[,] SudokuBoard { get; } = sudokuBoard;

    public int Dimension => (int)Math.Sqrt(SudokuBoard.Length);

    public int[,] Rows => SudokuBoard;
    public int[,] Cols
    {
        get
        {
            var cols = new int[SudokuBoard.Length, SudokuBoard.Length];
            for (int i = 0; i < SudokuBoard.Length; i++)
            for (int j = 0; j < SudokuBoard.Length; j++)
                cols[i,j] = SudokuBoard[j,i];
            return cols;
        }
    }

    public int[,] Qs
    {
        get
        {
            var dimensions = Dimension;
            var qs = new int[SudokuBoard.Length, SudokuBoard.Length];
            var qs1 = 0;
            for (var rowDiff = 0; rowDiff < dimensions; rowDiff++)
            for (var colDiff = 0; colDiff < dimensions; colDiff++)
            {
                var qs2 = 0;
                for (var i = rowDiff * dimensions; i < (rowDiff + 1) * dimensions; i++)
                for (var j = colDiff * dimensions; j < (colDiff + 1) * dimensions; j++)
                    qs[qs1, qs2++] = SudokuBoard[i, j];
                qs1++;
            }

            return qs;
        }
    }

    public string SudokuVisualize
    {
        get
        {
            var arr = new string[(int) Math.Pow(SudokuBoard.Length, 2)];
            var idx = 0;
            for (var i = 0; i < SudokuBoard.Length; i++)
            for (var j = 0; j < SudokuBoard.Length; j++)
                arr[idx++] = SudokuBoard[i, j].ToString();
            return string.Join(' ', arr);
        }
    }
    public Signature Signature { get; } = signature;
}
