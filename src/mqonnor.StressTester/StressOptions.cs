namespace mqonnor.StressTester;

public sealed class StressOptions
{
    public string Url { get; init; } = "http://localhost:8080/hubs/events";
    public int Events { get; init; } = 1000;
    public int Concurrency { get; init; } = 20;
    public int ExtraPayloadBytes { get; init; } = 0;
    public TimeSpan DrainTimeout { get; init; } = TimeSpan.FromSeconds(10);
    public string ReportPath { get; init; } = "stress-report.html";

    public static StressOptions FromArgs(string[] args)
    {
        var url = GetArg(args, "--url") ?? "http://localhost:8080/hubs/events";
        var events = int.TryParse(GetArg(args, "--events"), out var e) ? e : 1000;
        var concurrency = int.TryParse(GetArg(args, "--concurrency"), out var c) ? c : 20;
        var payloadSize = int.TryParse(GetArg(args, "--payload-size"), out var p) ? p : 0;
        var drainSec = int.TryParse(GetArg(args, "--drain-timeout"), out var d) ? d : 10;
        var report = GetArg(args, "--report") ?? "stress-report.html";

        return new StressOptions
        {
            Url = url,
            Events = events,
            Concurrency = concurrency,
            ExtraPayloadBytes = payloadSize,
            DrainTimeout = TimeSpan.FromSeconds(drainSec),
            ReportPath = report
        };
    }

    private static string? GetArg(string[] args, string name)
    {
        var idx = Array.IndexOf(args, name);
        return idx >= 0 && idx + 1 < args.Length ? args[idx + 1] : null;
    }
}
