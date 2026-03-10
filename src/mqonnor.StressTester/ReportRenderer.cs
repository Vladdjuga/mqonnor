using System.Text;
using System.Text.Json;

namespace mqonnor.StressTester;

public static class ReportRenderer
{
    public static void Write(StressReport report, string path)
    {
        var latencyLabels = BuildHistogramLabels(report.LatenciesMs);
        var latencyData = BuildHistogramData(report.LatenciesMs, latencyLabels);
        var cdfData = BuildCdfData(report.LatenciesMs);

        var minTs = report.ThroughputBuckets.Count > 0 ? report.ThroughputBuckets[0].BucketMs : 0;
        var throughputLabels = report.ThroughputBuckets
            .Select(b => $"+{(b.BucketMs - minTs) / 1000.0:F1}s")
            .ToList();
        var throughputData = report.ThroughputBuckets.Select(b => b.Count * 10).ToList(); // per-sec rate

        var html = $$"""
<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="UTF-8">
<title>mqonnor stress report</title>
<script src="https://cdn.jsdelivr.net/npm/chart.js@4"></script>
<style>
  body { font-family: system-ui, sans-serif; max-width: 1100px; margin: 2rem auto; padding: 0 1rem; background: #f8f9fa; color: #212529; }
  h1 { font-size: 1.6rem; }
  .grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(160px, 1fr)); gap: 1rem; margin: 1.5rem 0; }
  .card { background: #fff; border-radius: 8px; padding: 1rem 1.25rem; box-shadow: 0 1px 3px rgba(0,0,0,.1); }
  .card .value { font-size: 1.8rem; font-weight: 700; }
  .card .label { font-size: .8rem; color: #6c757d; margin-top: .2rem; }
  .charts { display: grid; grid-template-columns: 1fr 1fr; gap: 1.5rem; margin-top: 1.5rem; }
  .chart-box { background: #fff; border-radius: 8px; padding: 1.25rem; box-shadow: 0 1px 3px rgba(0,0,0,.1); }
  @media(max-width:700px){ .charts{ grid-template-columns:1fr; } }
</style>
</head>
<body>
<h1>mqonnor — stress test report</h1>
<p style="color:#6c757d">Generated {{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}} UTC</p>

<div class="grid">
  <div class="card"><div class="value">{{report.Sent}}</div><div class="label">Events sent</div></div>
  <div class="card"><div class="value">{{report.Received}}</div><div class="label">Events received</div></div>
  <div class="card"><div class="value" style="color:{{(report.Lost > 0 ? "#dc3545" : "#198754")}}">{{report.Lost}}</div><div class="label">Lost ({{report.LossPercent:F1}}%)</div></div>
  <div class="card"><div class="value">{{report.ProducerErrors}}</div><div class="label">Producer errors</div></div>
  <div class="card"><div class="value">{{report.ThroughputPerSec:F0}}</div><div class="label">Events/s</div></div>
  <div class="card"><div class="value">{{report.Elapsed.TotalSeconds:F1}}s</div><div class="label">Duration</div></div>
  <div class="card"><div class="value">{{report.P50:F0}} ms</div><div class="label">p50 latency</div></div>
  <div class="card"><div class="value">{{report.P95:F0}} ms</div><div class="label">p95 latency</div></div>
  <div class="card"><div class="value">{{report.P99:F0}} ms</div><div class="label">p99 latency</div></div>
  <div class="card"><div class="value">{{report.MaxMs:F0}} ms</div><div class="label">Max latency</div></div>
</div>

<div class="charts">
  <div class="chart-box"><canvas id="hist"></canvas></div>
  <div class="chart-box"><canvas id="cdf"></canvas></div>
  <div class="chart-box" style="grid-column:1/-1"><canvas id="throughput"></canvas></div>
</div>

<script>
const histLabels = {{JsonSerializer.Serialize(latencyLabels)}};
const histData   = {{JsonSerializer.Serialize(latencyData)}};
const cdfData    = {{JsonSerializer.Serialize(cdfData)}};
const tpLabels   = {{JsonSerializer.Serialize(throughputLabels)}};
const tpData     = {{JsonSerializer.Serialize(throughputData)}};

new Chart(document.getElementById('hist'), {
  type: 'bar',
  data: { labels: histLabels, datasets: [{ label: 'Events', data: histData, backgroundColor: '#0d6efd88' }] },
  options: { plugins: { title: { display: true, text: 'Latency distribution (ms)' } }, scales: { x: { title: { display: true, text: 'Latency (ms)' } }, y: { title: { display: true, text: 'Count' } } } }
});

new Chart(document.getElementById('cdf'), {
  type: 'line',
  data: { labels: cdfData.map(p => p.x + ' ms'), datasets: [{ label: 'CDF', data: cdfData.map(p => p.y), borderColor: '#198754', fill: false, pointRadius: 0 }] },
  options: { plugins: { title: { display: true, text: 'Cumulative distribution' } }, scales: { y: { min: 0, max: 100, title: { display: true, text: 'Percentile (%)' } } } }
});

new Chart(document.getElementById('throughput'), {
  type: 'line',
  data: { labels: tpLabels, datasets: [{ label: 'Events/s', data: tpData, borderColor: '#fd7e14', fill: true, backgroundColor: '#fd7e1422', pointRadius: 0, tension: 0.3 }] },
  options: { plugins: { title: { display: true, text: 'Throughput over time' } }, scales: { y: { title: { display: true, text: 'Events/s' } } } }
});
</script>
</body>
</html>
""";

        File.WriteAllText(path, html, Encoding.UTF8);
    }

    private static List<string> BuildHistogramLabels(IReadOnlyList<double> sorted)
    {
        if (sorted.Count == 0) return [];
        var max = sorted[^1];
        var bucketCount = Math.Min(40, sorted.Count);
        var size = Math.Max(1, (int)Math.Ceiling(max / bucketCount));
        return Enumerable.Range(0, bucketCount)
            .Select(i => $"{i * size}–{(i + 1) * size}")
            .ToList();
    }

    private static List<int> BuildHistogramData(IReadOnlyList<double> sorted, List<string> labels)
    {
        if (sorted.Count == 0) return [];
        var max = sorted[^1];
        var size = Math.Max(1, (int)Math.Ceiling(max / labels.Count));
        return labels.Select((_, i) =>
            sorted.Count(v => v >= i * size && v < (i + 1) * size)).ToList();
    }

    private static List<object> BuildCdfData(IReadOnlyList<double> sorted)
    {
        if (sorted.Count == 0) return [];
        return sorted
            .Select((v, i) => (object)new { x = (int)v, y = Math.Round((i + 1.0) / sorted.Count * 100, 1) })
            .ToList();
    }
}
