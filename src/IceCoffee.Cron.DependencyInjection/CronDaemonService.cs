using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IceCoffee.Cron.DependencyInjection;

public class CronDaemonService : IHostedService
{
    private readonly ICronDaemon _cronDaemon;
    private readonly ILogger<CronDaemonService> _logger;
    private readonly IOptionsMonitor<CronJobOptions> _optionsMonitor;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public CronDaemonService(ICronDaemon cronDaemon, ILogger<CronDaemonService> logger, IOptionsMonitor<CronJobOptions> optionsMonitor, IHostApplicationLifetime hostApplicationLifetime)
    {
        _cronDaemon = cronDaemon;
        _logger = logger;
        _optionsMonitor = optionsMonitor;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        _hostApplicationLifetime.ApplicationStarted.Register(OnApplicationStarted);

        _cronDaemon.Start();
        _logger.LogInformation("Cron daemon started.");
        return Task.CompletedTask;
    }

    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        _cronDaemon.Stop();
        _logger.LogInformation("Cron daemon stopped.");
        return Task.CompletedTask;
    }

    private async void OnApplicationStarted()
    {
        var jobsToRun = _cronDaemon.CronJobs.Values
            .Where(job =>
            {
                var options = _optionsMonitor.Get(job.Name);
                return options.IsEnabled && options.RunOnceAtStart;
            });

        if (jobsToRun.Any())
        {
            _logger.LogInformation("Running jobs once at start...");
            foreach (var job in jobsToRun)
            {
                await SafeRunAsync(job.Action);
            }
        }
    }

    private async Task SafeRunAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        //catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        //{
        //    _logger.LogWarning("Job execution was canceled.");
        //}
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while running a job at start.");
        }
    }
}
