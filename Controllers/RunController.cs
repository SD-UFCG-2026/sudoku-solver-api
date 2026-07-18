using System.Net;
using Microsoft.AspNetCore.Mvc;
using SudokuSolverAPI.DTOs;

namespace SudokuSolverAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class RunController : ControllerBase
{

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        return StatusCode(StatusCodes.Status503ServiceUnavailable);
    }

    [HttpPost("{id}")]
    public IActionResult Post()
    {
        return NoContent();
    }
}
