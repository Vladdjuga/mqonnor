using System.Diagnostics;

namespace mqonnor.API.Services;

public sealed class ThreadPoolMonitor(ILogger<ThreadPoolMonitor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            ThreadPool.GetAvailableThreads(out var workerAvailable, out var ioAvailable);
            ThreadPool.GetMinThreads(out var workerMin, out var ioMin);
            ThreadPool.GetMaxThreads(out var workerMax, out var ioMax);

            var workerInUse = workerMax - workerAvailable;
            var ioInUse = ioMax - ioAvailable;

            var process = Process.GetCurrentProcess();
            var threadCount = process.Threads.Count;

            logger.LogInformation(
                "[ThreadPool] Workers: {WorkerInUse}/{WorkerMax} (min {WorkerMin}) | IO: {IoInUse}/{IoMax} (min {IoMin}) | Total threads: {ThreadCount}",
                workerInUse, workerMax, workerMin,
                ioInUse, ioMax, ioMin,
                threadCount);

            // Check for starvation warning
            if (workerAvailable < workerMin / 2)
            {
                logger.LogWarning("[ThreadPool] ⚠️ POSSIBLE STARVATION — only {Available} worker threads available!", workerAvailable);
            }

            await Task.Delay(2000, stoppingToken);
        }
    }
}
