using Cronos;

namespace IceCoffee.Cron
{
    /// <summary>
    /// Interface for Cron Job.
    /// </summary>
    public interface ICronJob
    {
        /// <summary>
        /// Job name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Cron expression.
        /// </summary>
        CronExpression CronExpression { get; }

        /// <summary>
        /// Whether the job is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Whether the job allows concurrent execution.
        /// </summary>
        bool AllowConcurrentExecution { get; }

        /// <summary>
        /// Execute the job.
        /// </summary>
        /// <param name="currentTime"></param>
        void Execute(DateTime currentTime);
    }
}
