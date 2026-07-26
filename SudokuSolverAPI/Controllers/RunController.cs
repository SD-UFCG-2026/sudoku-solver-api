using System.Net;
using Microsoft.AspNetCore.Mvc;
using SudokuSolverAPI.Channels;
using SudokuSolverAPI.DTOs;
using SudokuSolverAPI.Interfaces;

namespace SudokuSolverAPI.Controllers;

[ApiController]
[Route("api/sudoku/")]
public class RunController(
    ValidationChannel validationChannel,
    IBoardPersisterService persisterService,
    ILogger<RunController> logger) : ControllerBase
{

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        try
        {
            var run = await persisterService.Get(id);
            return Ok(run.toDTO());
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}")]
    public IActionResult Post(
        int id,
        [FromBody] BoardDto boardDto)
    {
        if (!validationChannel.Writer.TryWrite(new ValidationData(id, boardDto)))
        {
            logger.LogInformation($"Request from: {boardDto.Signature.Identifier} was lost from Validation Channel.");
            return StatusCode(StatusCodes.Status429TooManyRequests);
        }

        return Accepted();
    }

    [HttpGet()]
    public async Task<IActionResult> GetAll()
    {
        var allEntites = await persisterService.GetAll();

        foreach (var entities in allEntites)
        {
            entities.Root.Nodes.Clear();
        }

        return Ok(allEntites
            .Select(e => e.toDTO()));
    }
}
