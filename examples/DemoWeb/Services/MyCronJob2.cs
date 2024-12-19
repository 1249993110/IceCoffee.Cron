using IceCoffee.Cron;
using IceCoffee.Cron.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DemoWeb.Services
{
    public class MyCronJob2 : CronJobService
    {
        public MyCronJob2(ICronDaemon cronDaemon, IOptionsMonitor<CronJobOptions> optionsMonitor) : base(cronDaemon, optionsMonitor)
        {
        }

        public override Task Execute()
        {
            Console.WriteLine($"Job2 executed at {DateTime.Now}");
            return Task.CompletedTask;
        }
    }
}
