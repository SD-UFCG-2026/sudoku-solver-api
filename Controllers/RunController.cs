using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace SudokuSolverAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class RunController : ControllerBase
{

    [HttpGet]
    public IActionResult Get()
    {
        return StatusCode(StatusCodes.Status503ServiceUnavailable);
    }

    [HttpPost]
    public IActionResult Post()
    {
        return NoContent();
    }
}
