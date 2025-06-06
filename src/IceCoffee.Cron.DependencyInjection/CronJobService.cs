﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace IceCoffee.Cron.DependencyInjection;

public abstract class CronJobService : IHostedService
{
    private readonly ICronDaemon _cronDaemon;
    private readonly IOptionsMonitor<CronJobOptions> _optionsMonitor;

    public ICronDaemon CronDaemon => _cronDaemon;
    public IOptionsMonitor<CronJobOptions> OptionsMonitor => _optionsMonitor;
    public CronJob CronJob => _cronDaemon.CronJobs[Name];
    public string Name { get; internal set; } = null!;

    public CronJobService(ICronDaemon cronDaemon, IOptionsMonitor<CronJobOptions> optionsMonitor)
    {
        _cronDaemon = cronDaemon;
        _optionsMonitor = optionsMonitor;
        _optionsMonitor.OnChange(OnOptionsChange);
    }

    private static TimeZoneInfo GetTimeZoneInfo(string? timeZone)
    {
        return timeZone == null ? TimeZoneInfo.Local : TimeZoneInfo.FindSystemTimeZoneById(timeZone);
    }

    private void OnOptionsChange(CronJobOptions newOptions, string? name)
    {
        if (name == Name)
        {
            if (newOptions.IsEnabled)
            {
                var job = new CronJob(Name, newOptions.CronExpression, Execute, GetTimeZoneInfo(newOptions.TimeZone), newOptions.AllowConcurrentExecution);
                _cronDaemon.AddJob(job);
            }
            else
            {
                _cronDaemon.RemoveJob(Name);
            }
        }
    }

    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        var options =  _optionsMonitor.Get(Name);
        if (options.IsEnabled)
        {
            var job = new CronJob(Name, options.CronExpression, Execute, GetTimeZoneInfo(options.TimeZone), options.AllowConcurrentExecution);
            _cronDaemon.AddJob(job);
        }

        return Task.CompletedTask;
    }

    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        _cronDaemon.RemoveJob(Name);
        return Task.CompletedTask;
    }

    public abstract Task Execute();
}
