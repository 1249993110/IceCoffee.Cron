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
        try
        {
            var filteredJobs = new List<(CronJob job, CronJobOptions options)>();

            foreach (var job in _cronDaemon.CronJobs.Values)
            {
                var options = _optionsMonitor.Get(job.Name);
                if (options.IsEnabled && options.RunOnceAtStart)
                {
                    filteredJobs.Add((job, options));
                }
            }

            var jobsToRun = filteredJobs
                .OrderBy(x => x.options.RunOrderAtStart)
                .Select(x => x.job);

            if (jobsToRun.Any())
            {
                _logger.LogInformation("Running jobs once at start...");
                foreach (var job in jobsToRun)
                {
                    await SafeRunAsync(job.Action);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CronDaemonService.OnApplicationStarted.");
        }
    }

    private async Task SafeRunAsync(Func<Task> action)
    {
        try
        {
            await action.Invoke();
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
