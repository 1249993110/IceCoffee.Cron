using Microsoft.Extensions.Hosting;

namespace IceCoffee.Cron.DependencyInjection;

public class CronDaemonService : IHostedService
{
    private readonly ICronDaemon _cronDaemon;

    public CronDaemonService(ICronDaemon cronDaemon)
    {
        _cronDaemon = cronDaemon;
    }

    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        _cronDaemon.Start();
        return Task.CompletedTask;
    }

    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        _cronDaemon.Stop();
        return Task.CompletedTask;
    }
}
