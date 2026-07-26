namespace SudokuSolverAPI.DTOs;

public record RunDto(int id, NodeDto Root, bool IsFinished, NodeDto? Final)
{
}

/*
 *  {
 *      root: {Node que você já viu},
 *      isFinished: true/false,
 *      final: null/node final
 *  }
 */