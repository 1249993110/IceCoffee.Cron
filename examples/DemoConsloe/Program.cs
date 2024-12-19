using IceCoffee.Cron;

namespace Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Create CronDaemon instance
            ICronDaemon daemon = new CronDaemon();

            // Add jobs
            daemon.AddJob(new CronJob(
                "Job1",
                "* * * * * *", // Run every second
                () =>
                {
                    Console.WriteLine($"Job1 executed at {DateTime.Now}");
                }));

            daemon.AddJob(new CronJob(
                "Job2",
                "0/5 * * * * ?", // Run every 5 seconds
                async () =>
                {
                    Console.WriteLine($"Job2 executed at {DateTime.Now}");
                    await Task.Delay(100); // Simulate job execution
                }));

            daemon.AddJob(new CronJob(
                "Job3",
                "0 * * * * ?", // Run at the 0th second of every minute
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
