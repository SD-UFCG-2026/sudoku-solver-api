using System.Threading.Channels;

namespace SudokuSolverAPI.Channels;

public class ProcessingChannel(IConfiguration configuration)
{
    private readonly Channel<ProcessingData> _channel = Channel.CreateBounded<ProcessingData>(
        new BoundedChannelOptions(
            configuration.GetValue("PROCESSING_CHANNEL_CAPACITY", 1000))
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = false,
            SingleReader = true
        });

    public ChannelWriter<ProcessingData> Writer => _channel.Writer;
    public ChannelReader<ProcessingData> Reader => _channel.Reader;
}

public record ProcessingData(int id, BoardNode node) {}