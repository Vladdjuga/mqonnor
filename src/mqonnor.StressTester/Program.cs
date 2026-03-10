using System.Diagnostics;
using mqonnor.StressTester;
using Spectre.Console;

var options = StressOptions.FromArgs(args);

AnsiConsole.Write(new FigletText("mqonnor").Color(Color.DodgerBlue1));
AnsiConsole.MarkupLine("[grey]stress tester[/]");
AnsiConsole.WriteLine();

AnsiConsole.MarkupLine($"[bold]URL:[/]         {options.Url}");
AnsiConsole.MarkupLine($"[bold]Events:[/]      {options.Events}");
AnsiConsole.MarkupLine($"[bold]Concurrency:[/] {options.Concurrency}");
AnsiConsole.MarkupLine($"[bold]Drain timeout:[/] {options.DrainTimeout.TotalSeconds}s");
AnsiConsole.MarkupLine($"[bold]Report:[/]      {options.ReportPath}");
AnsiConsole.WriteLine();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

var producer = new EventProducer(options);
var consumer = new EventConsumer(options);

await AnsiConsole.Status().StartAsync("Connecting...", async ctx =>
{
    ctx.Spinner(Spinner.Known.Dots);
    await consumer.ConnectAsync(cts.Token);
    await producer.ConnectAsync(cts.Token);
    ctx.Status("Connected");
    await Task.Delay(300, cts.Token); // let SignalR settle
});

var sw = Stopwatch.StartNew();

await AnsiConsole.Progress()
    .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new SpinnerColumn())
    .StartAsync(async ctx =>
    {
        var progressTask = ctx.AddTask("Publishing events", maxValue: options.Events);

        var producerTask = producer.RunAsync(cts.Token).ContinueWith(_ =>
        {
            progressTask.Value = options.Events;
        });

        // Update progress bar periodically while publishing
        while (!producerTask.IsCompleted)
        {
            progressTask.Value = producer.Sent.Count;
            await Task.Delay(150);
        }
        await producerTask;
    });

sw.Stop();

AnsiConsole.MarkupLine($"\n[green]Publishing done[/] — {producer.Sent.Count} events sent in {sw.Elapsed.TotalSeconds:F2}s");
AnsiConsole.MarkupLine($"Waiting up to [yellow]{options.DrainTimeout.TotalSeconds}s[/] for consumer to catch up...");

var deadline = DateTime.UtcNow + options.DrainTimeout;
while (consumer.Received.Count < producer.Sent.Count && DateTime.UtcNow < deadline)
    await Task.Delay(200);

await producer.DisposeAsync();
await consumer.DisposeAsync();

var report = StressReport.Build(producer.Sent, consumer.Received, producer.Errors, sw.Elapsed);

// Print summary table
var table = new Table().Border(TableBorder.Rounded).AddColumn("Metric").AddColumn("Value");
table.AddRow("Sent", report.Sent.ToString());
table.AddRow("Received", report.Received.ToString());
table.AddRow("Lost", $"[{(report.Lost > 0 ? "red" : "green")}]{report.Lost} ({report.LossPercent:F1}%)[/]");
table.AddRow("Producer errors", report.ProducerErrors.ToString());
table.AddRow("Throughput", $"{report.ThroughputPerSec:F0} events/s");
table.AddRow("p50 latency", $"{report.P50:F0} ms");
table.AddRow("p95 latency", $"{report.P95:F0} ms");
table.AddRow("p99 latency", $"{report.P99:F0} ms");
table.AddRow("Max latency", $"{report.MaxMs:F0} ms");
AnsiConsole.Write(table);

ReportRenderer.Write(report, options.ReportPath);
AnsiConsole.MarkupLine($"\n[bold green]Report written → {options.ReportPath}[/]");
