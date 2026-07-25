using System.Net;
using Microsoft.AspNetCore.Mvc;
using SudokuSolverAPI.Channels;
using SudokuSolverAPI.DTOs;
using SudokuSolverAPI.Interfaces;

namespace SudokuSolverAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class RunController(
    ValidationChannel validationChannel,
    IBoardPersisterService persisterService) : ControllerBase
{

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var run = await persisterService.Get(id);
        return Ok(run.toDTO());
    }

    [HttpPost("{id}")]
    public IActionResult Post(
        int id,
        [FromBody] BoardDto boardDto)
    {
        if (!validationChannel.Writer.TryWrite(new ValidationData(id, boardDto)))
        {
            return StatusCode(StatusCodes.Status429TooManyRequests);
        }

        return Accepted();
    }
}
