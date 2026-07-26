using Microsoft.Extensions.Logging.Abstractions;
using SudokuSolverAPI.BackgroundServices;
using SudokuSolverAPI.Channels;
using SudokuSolverAPI.DTOs;
using SudokuSolverAPI.Services;

namespace SudokuSolverAPI.Tests.Integration.BackgroundServices;

public class ValidationBackgroundServiceIntegrationTests : MongoDbIntegrationTestBase
{
    private readonly Signature? _dummySignature = null;

    private ValidationChannel _validationChannel = null!;
    private ProcessingChannel _processingChannel = null!;
    private BoardPersisterService _persisterService = null!;
    private BoardValidatorService _validatorService = null!;
    private ValidationBackgroundService _backgroundService = null!;
    private CancellationTokenSource _backgroundCts = null!;

    private readonly BoardRun _defaultRun;

    public ValidationBackgroundServiceIntegrationTests()
    {
        int[,] rootBoardData = {
            { 1, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };
        var rootNode = new BoardNode(new Board(rootBoardData, _dummySignature!));
        _defaultRun = new BoardRun(0, rootNode) { Id = 1 };
    }

    protected override Dictionary<string, string?> GetCustomConfiguration() => new()
    {
        {"VALIDATION_WORKER_COUNT", "2"},
        {"VALIDATION_CHANNEL_CAPACITY", "10"},
        {"PROCESSING_CHANNEL_CAPACITY", "10"}
    };

    public override async Task InitializeAsync()
    {
        // Sobe o banco de dados da classe base
        await base.InitializeAsync();

        _validationChannel = new ValidationChannel(Configuration);
        _processingChannel = new ProcessingChannel(Configuration);

        _persisterService = new BoardPersisterService(Database, Configuration);
        _validatorService = new BoardValidatorService();

        _backgroundService = new ValidationBackgroundService(
            _validationChannel,
            _processingChannel,
            _validatorService,
            _persisterService,
            Configuration,
            NullLogger<ValidationBackgroundService>.Instance
        );

        _backgroundCts = new CancellationTokenSource();

        await _persisterService.SaveRun(_defaultRun);
        await _backgroundService.StartAsync(_backgroundCts.Token);
    }

    public override async Task DisposeAsync()
    {
        _backgroundCts.Cancel();
        await _backgroundService.StopAsync(CancellationToken.None);

        await base.DisposeAsync();
    }

    [Fact]
    public async Task ValidBoard_ShouldProcessAndForward()
    {
        int[,] validEvolutionData = {
            { 1, 2, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        await SendToChannel(validEvolutionData);
        var output = await ReadFromChannelWithTimeout();

        Assert.NotNull(output);
        Assert.Equal(_defaultRun.Id, output.id);
    }

    [Fact]
    public async Task InvalidBoard_RowConflict_ShouldDiscard()
    {
        int[,] invalidData = {
            { 1, 1, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        await SendToChannel(invalidData);
        Assert.True(await ConfirmEmptyChannel());
    }

    [Fact]
    public async Task InvalidBoard_ColumnConflict_ShouldDiscard()
    {
        int[,] invalidData = {
            { 1, 0, 0, 0 },
            { 1, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        await SendToChannel(invalidData);
        Assert.True(await ConfirmEmptyChannel());
    }

    [Fact]
    public async Task InvalidBoard_QuadrantConflict_ShouldDiscard()
    {
        int[,] invalidData = {
            { 1, 0, 0, 0 },
            { 0, 1, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        await SendToChannel(invalidData);
        Assert.True(await ConfirmEmptyChannel());
    }

    [Fact]
    public async Task InvalidBoard_CausalityViolation_ShouldDiscard()
    {
        int[,] invalidData = {
            { 2, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        await SendToChannel(invalidData);
        Assert.True(await ConfirmEmptyChannel());
    }

    [Fact]
    public async Task InvalidBoard_DifferentDimension_ShouldDiscard()
    {
        int[,] invalidData = new int[9,9];
        invalidData[0,0] = 1;

        await SendToChannel(invalidData);
        Assert.True(await ConfirmEmptyChannel());
    }

    [Fact]
    public async Task DuplicateBoard_ShouldForwardFirstAndDiscardSecond()
    {
        int[,] validEvolutionData = {
            { 1, 2, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };

        await SendToChannel(validEvolutionData);
        var firstOutput = await ReadFromChannelWithTimeout();
        Assert.NotNull(firstOutput);

        await SendToChannel(validEvolutionData);
        Assert.True(await ConfirmEmptyChannel(), "Dual board wasn't stopped by validation service");
    }

    [Fact]
    public async Task UnknownRunId_ShouldCatchExceptionAndNotCrashBackgroundService()
    {
        int[,] validEvolutionData = {
            { 1, 2, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        };
        var validDto = new BoardDto(validEvolutionData, _dummySignature!);

        var validationMessage = new ValidationData(999, validDto);
        await _validationChannel.Writer.WriteAsync(validationMessage);

        Assert.True(await ConfirmEmptyChannel());
    }

    private async Task SendToChannel(int[,] boardData, int? runId = null)
    {
        var dto = new BoardDto(boardData, _dummySignature!);
        var message = new ValidationData(runId ?? _defaultRun.Id, dto);
        await _validationChannel.Writer.WriteAsync(message);
    }

    private async Task<ProcessingData> ReadFromChannelWithTimeout()
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        try
        {
            return await _processingChannel.Reader.ReadAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException)
        {
            Assert.Fail("Timeout on reading channel");
            throw;
        }
    }

    private async Task<bool> ConfirmEmptyChannel(int delayMs = 200)
    {
        await Task.Delay(delayMs);
        var temMensagem = _processingChannel.Reader.TryPeek(out _);
        var quantidade = _processingChannel.Reader.Count;
        return !temMensagem && quantidade == 0;
    }
}