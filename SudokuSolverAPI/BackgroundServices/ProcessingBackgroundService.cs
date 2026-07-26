using System.Collections.Concurrent;
using SudokuSolverAPI.Channels;
using SudokuSolverAPI.Interfaces;

namespace SudokuSolverAPI.BackgroundServices;

public class ProcessingBackgroundService(
    ProcessingChannel processingChannel,
    IBoardProcesserService boardProcesserService,
    IBoardPersisterService boardPersisterService,
    ILogger<ProcessingBackgroundService> logger): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Processing begun");

        ConcurrentDictionary<int, Lazy<Task<BoardRun>>> roots = [];

        await foreach (var dto in processingChannel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {

                var lazyRunTask = roots.GetOrAdd(dto.id, id =>
                    new Lazy<Task<BoardRun>>(() => boardPersisterService.Get(id)));

                var run = await lazyRunTask.Value;

                var result = boardProcesserService.Process(run, dto.node);
                await boardPersisterService.SaveRun(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Process error");
            }
        }
    }
}