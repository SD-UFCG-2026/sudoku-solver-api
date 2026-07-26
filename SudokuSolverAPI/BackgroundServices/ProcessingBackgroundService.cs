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
                logger.LogInformation($"Request from ${dto.node.Value.Signature.Identifier} in run ${dto.id} was processed successfully");
                await boardPersisterService.SaveRun(result);
                logger.LogInformation($"Request from ${dto.node.Value.Signature.Identifier} in run ${dto.id} was persisted successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Process error");
            }
        }
    }
}