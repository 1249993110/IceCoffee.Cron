using IceCoffee.Cron;
using IceCoffee.Cron.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DemoWeb.Services
{
    public class MyCronJob1 : CronJobService
    {
        public MyCronJob1(ICronDaemon cronDaemon, IOptionsMonitor<CronJobOptions> optionsMonitor) : base(cronDaemon, optionsMonitor)
        {
        }

        public override Task Execute()
        {
            Console.WriteLine($"Job1 executed at {DateTime.Now}");
            return Task.CompletedTask;
        }
    }
}
