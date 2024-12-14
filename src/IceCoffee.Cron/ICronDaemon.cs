namespace IceCoffee.Cron
{
    /// <summary>
    /// Interface for Cron Daemon.
    /// </summary>
    public interface ICronDaemon
    {
        /// <summary>
        /// All jobs in the daemon.
        /// </summary>
        IReadOnlyDictionary<string, ICronJob> CronJobs { get; }

        /// <summary>
        /// Add a job to the daemon. If the job name already exists, it will be overwritten.
        /// </summary>
        /// <param name="cronJob"></param>
        void AddJob(ICronJob cronJob);

        /// <summary>
        /// Remove a job from the daemon.
        /// </summary>
        /// <param name="cronJob"></param>
        bool RemoveJob(ICronJob cronJob);

        /// <summary>
        /// Remove a job from the daemon.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool RemoveJob(string name);

        /// <summary>
        /// Start the daemon.
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the daemon.
        /// </summary>
        void Stop();
    }
}
