using System.Collections.Concurrent;

namespace mqonnor.StressTester;

public sealed class StressReport
{
    public int Sent { get; init; }
    public int Received { get; init; }
    public int ProducerErrors { get; init; }
    public TimeSpan Elapsed { get; init; }

    public IReadOnlyList<double> LatenciesMs { get; init; } = [];
    // Throughput over time: (bucketStartMs, eventsInBucket)
    public IReadOnlyList<(long BucketMs, int Count)> ThroughputBuckets { get; init; } = [];

    public int Lost => Sent - Received;
    public double LossPercent => Sent == 0 ? 0 : (double)Lost / Sent * 100;
    public double ThroughputPerSec => Elapsed.TotalSeconds == 0 ? 0 : Received / Elapsed.TotalSeconds;

    public double P50 => Percentile(50);
    public double P95 => Percentile(95);
    public double P99 => Percentile(99);
    public double MaxMs => LatenciesMs.Count == 0 ? 0 : LatenciesMs[^1];
    public double MinMs => LatenciesMs.Count == 0 ? 0 : LatenciesMs[0];
    public double MeanMs => LatenciesMs.Count == 0 ? 0 : LatenciesMs.Average();

    private double Percentile(int p)
    {
        if (LatenciesMs.Count == 0) return 0;
        var idx = (int)Math.Ceiling(p / 100.0 * LatenciesMs.Count) - 1;
        return LatenciesMs[Math.Clamp(idx, 0, LatenciesMs.Count - 1)];
    }

    public static StressReport Build(
        ConcurrentDictionary<Guid, long> sent,
        ConcurrentDictionary<Guid, long> received,
        int producerErrors,
        TimeSpan elapsed)
    {
        var latencies = sent
            .Where(kv => received.TryGetValue(kv.Key, out _))
            .Select(kv => (double)(received[kv.Key] - kv.Value))
            .OrderBy(x => x)
            .ToList();

        // 100 ms throughput buckets
        var buckets = received.Values
            .GroupBy(ts => ts / 100 * 100)
            .Select(g => (BucketMs: g.Key, Count: g.Count()))
            .OrderBy(x => x.BucketMs)
            .ToList();

        return new StressReport
        {
            Sent = sent.Count,
            Received = received.Count,
            ProducerErrors = producerErrors,
            Elapsed = elapsed,
            LatenciesMs = latencies,
            ThroughputBuckets = buckets
        };
    }
}
