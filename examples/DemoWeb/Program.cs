using DemoWeb.Services;
using IceCoffee.Cron.DependencyInjection;

namespace DemoWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var services = builder.Services;
            services.AddCronJob<MyCronJob1>(options =>
            {
                options.TimeZone = TimeZoneInfo.Local.Id;
                options.CronExpression = "* * * * * ?";
            });

            services.AddCronJob<MyCronJob2>(builder.Configuration.GetSection("CronJobOptions"));

            var app = builder.Build();
            
            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}
