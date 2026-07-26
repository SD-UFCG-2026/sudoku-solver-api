using System.Threading.Channels;
using SudokuSolverAPI.DTOs;

namespace SudokuSolverAPI.Channels;

public class ValidationChannel(IConfiguration configuration)
{
    private readonly Channel<ValidationData> _channel = Channel.CreateBounded<ValidationData>(
        new BoundedChannelOptions(
            configuration.GetValue("VALIDATION_CHANNEL_CAPACITY",1000))
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = false,
            SingleReader = false
        });

    public ChannelWriter<ValidationData> Writer => _channel.Writer;
    public ChannelReader<ValidationData> Reader => _channel.Reader;

}

public record ValidationData(int id, BoardDto board) {}