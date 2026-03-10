using System.Threading.Channels;
using mqonnor.Domain.Entities;

namespace mqonnor.Infra.Extensions;

public static class ChannelExtensions
{
    public static async ValueTask<int> ReadBatchAsync<T>(
        this ChannelReader<T> reader,
        List<T> batch,
        int maxSize,
        CancellationToken ct = default)
    {
        if (!await reader.WaitToReadAsync(ct))
            return 0;

        var count = 0;
        while (count < maxSize && reader.TryRead(out var item))
        {
            batch.Add(item);
            count++;
        }

        return count;
    }
}