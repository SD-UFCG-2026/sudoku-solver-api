namespace SudokuSolverAPI.DTOs;

public record NodeDto(BoardDto Value, List<NodeDto> Child);

/*
 *  {
 *      value: {Board que ja vimos antes},
 *      child: [{
 *          value: {Outro board},
 *          child: [...]
 *     }, ...]
 *  }
 */