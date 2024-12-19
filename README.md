# IceCoffee.Cron

| Package | NuGet Stable | Downloads |
| ------- | ------------ | --------- |
| [IceCoffee.Cron](https://www.nuget.org/packages/IceCoffee.Cron/) | [![IceCoffee.Cron](https://img.shields.io/nuget/v/IceCoffee.Cron.svg)](https://www.nuget.org/packages/IceCoffee.Cron/) | [![IceCoffee.Cron](https://img.shields.io/nuget/dt/IceCoffee.Cron.svg)](https://www.nuget.org/packages/IceCoffee.Cron/) |

## Description

IceCoffee.Cron is a simple C# library for running tasks based on a cron schedule.

## Installation

```sh
$ dotnet add package IceCoffee.Cron
$ dotnet add package IceCoffee.Cron.DependencyInjection # (optional) If you want use DI
```

## Cron Schedules

IceCoffee.Cron supports most cron scheduling. See tests for supported formats.

```
*   *   *   *   *   *                         Allowed values     Allowed special characters     Comment
┬   ┬   ┬   ┬   ┬   ┬
│   │   │   │   │   │
│   │   │   │   │   │
│   │   │   │   │   └────── day of week       0 - 6  or SUN-SAT  * , - /                        Both 0 and 7 means SUN
│   │   │   │   └────────── month             1 - 12 or JAN-DEC  * , - /                      
│   │   │   └────────────── day of month      1 - 31             * , - /                      
│   │   └────────────────── hour              0 - 23             * , - / L W ?                
│   └────────────────────── min               0 - 59             * , - /                      
└────────────────────────── second (optional) 0 - 59             * , - / # L ?                  
```

| Expression            | Description                                                                           |
|-----------------------|---------------------------------------------------------------------------------------|
| `*/1 * * * * *`       | Every second                                                                          |
| `* * * * *`           | Every minute                                                                          |
| `0 0 1 * *`           | At midnight, on day 1 of every month                                                  |
| `*/5 * * * *`         | Every 5 minutes                                                                       |
| `30,45-15/2 1 * * *`  | Every 2 minute from 1:00 AM to 01:15 AM and from 1:45 AM to 1:59 AM and at 1:30 AM    |
| `0 0 * * MON-FRI`     | At 00:00, Monday through Friday                                                       |
| `0 * * * *`           | Every minute                                                                          |
| `0,1,2 * * * *`       | Top of every hour                                                                     |
| `*/2 * * * *`         | Every hour at minutes 0, 1, and 2                                                     |
| `1-55 * * * *`        | Every minute through the 55th minute                                                  |
| `* 1,10,20 * * *`     | Every 1st, 10th, and 20th hours2                                                      |


Console Example
===============

```csharp
using IceCoffee.Cron;

namespace Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Create CronDaemon instance
            var daemon = new CronDaemon();

            // Add jobs
            daemon.AddJob(new CronJob(
                "Job1",
                "*/1 * * * * *", // Run every second
                () =>
                {
                    Console.WriteLine($"Job1 executed at {DateTime.Now}");
                }));

            daemon.AddJob(new CronJob(
                "Job2",
                "*/5 * * * * *", // Run every 5 seconds
                async () =>
                {
                    Console.WriteLine($"Job2 executed at {DateTime.Now}");
                    await Task.Delay(100); // Simulate job execution
                }));

            daemon.AddJob(new CronJob(
                "Job3",
                "0 */1 * * * *", // Run at the 0th second of every minute
                async () =>
                {
                    Console.WriteLine($"Job3 executed at {DateTime.Now}");
                    await Task.Delay(100); // Simulate job execution
                }));

            // Start CronDaemon
            daemon.Start();
            Console.WriteLine($"CronDaemon started at {DateTime.Now}");

            // Wait for user input to stop the program
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();

            // Stop CronDaemon
            daemon.Stop();
            Console.WriteLine($"CronDaemon stopped at {DateTime.Now}");
            Console.ReadKey();
        }
    }
}
```

### Example with Asp.Net Core and D.I

#### Implements CronJobService

```csharp
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

```

#### Configure Services

```csharp
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
                options.CronExpression = "0/5 * * * * ?";
            });

            services.AddCronJob<MyCronJob2>(builder.Configuration.GetSection("CronJobOptions"));

            var app = builder.Build();
            
            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}

```