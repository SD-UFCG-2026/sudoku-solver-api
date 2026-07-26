using System.Collections.Concurrent;
using SudokuSolverAPI.Channels;
using SudokuSolverAPI.Interfaces;

namespace SudokuSolverAPI.BackgroundServices;

public class ValidationBackgroundService(
    ValidationChannel validationChannel,
    ProcessingChannel processingChannel,
    IBoardValidatorService boardValidatorService,
    IBoardPersisterService boardPersisterService,
    IConfiguration configuration,
    ILogger<ValidationBackgroundService> logger
    ) : BackgroundService
{

    private readonly int _workerCount = configuration.GetValue(
        "VALIDATION_WORKER_COUNT",Environment.ProcessorCount);
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation($"Validation begun with {_workerCount} workers");

        ConcurrentDictionary<int, Lazy<Task<BoardRun>>> roots = [];

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = _workerCount,
            CancellationToken = stoppingToken
        };

        await Parallel.ForEachAsync(
            validationChannel.Reader.ReadAllAsync(stoppingToken),
            parallelOptions,
            async (dto, ctx) =>
            {
                try
                {
                    var lazyRunTask = roots.GetOrAdd(dto.id, id =>
                        new Lazy<Task<BoardRun>>(() => boardPersisterService.Get(id)));

                    var run = await lazyRunTask.Value;

                    if (boardValidatorService.IsValid(run, dto.board.ToEntity()))
                    {
                        logger.LogInformation($"Board contributed from ${dto.board.Signature.Identifier} was validated on run ${dto.id}");
                        await processingChannel.Writer.WriteAsync(
                            new ProcessingData(dto.id, new BoardNode(dto.board.ToEntity()))
                            , ctx);
                    }
                    else
                    {
                        logger.LogError($"Invalid board from: ${dto.board.Signature.Identifier}.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Validation error");
                }
            });
    }
}