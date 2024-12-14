using IceCoffee.Cron;

namespace Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 创建 CronDaemon
            var daemon = new CronDaemon();

            // 添加任务
            daemon.AddJob(new CronJob(
                "Job1",
                "*/1 * * * * *", // 每秒运行一次
                () =>
                {
                    Console.WriteLine($"Job1 executed at {DateTime.Now}");
                }));

            daemon.AddJob(new CronJob(
                "Job2",
                "*/5 * * * * *", // 每 5 秒运行一次
                async () =>
                {
                    Console.WriteLine($"Job2 executed at {DateTime.Now}");
                    await Task.Delay(100); // 模拟任务执行
                }));

            daemon.AddJob(new CronJob(
                "Job3",
                "0 */1 * * * *", // 每分钟的第 0 秒运行
                async () =>
                {
                    Console.WriteLine($"Job3 executed at {DateTime.Now}");
                    await Task.Delay(100); // 模拟任务执行
                }));

            // 启动 CronDaemon
            daemon.Start();
            Console.WriteLine($"CronDaemon started at {DateTime.Now}");

            // 等待用户输入停止程序
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();

            // 停止 CronDaemon
            daemon.Stop();
            Console.WriteLine($"CronDaemon stopped at {DateTime.Now}");
            Console.ReadKey();
        }
    }
}
