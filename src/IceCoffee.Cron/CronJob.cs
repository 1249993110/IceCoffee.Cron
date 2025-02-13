using Cronos;

namespace IceCoffee.Cron
{
    public class CronJob
    {
        private readonly Func<Task> _action;
        private DateTime? _nextRunTime;
        private volatile int _runningFlag;

        /// <summary>
        /// Job name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Cron expression.
        /// </summary>
        public CronExpression CronExpression { get; private set; }

        /// <summary>
        /// Whether the job is running.
        /// </summary>
        public bool IsRunning => _runningFlag > 0;

        /// <summary>
        /// Whether the job allows concurrent execution.
        /// </summary>
        public bool AllowConcurrentExecution { get; set; } = true;

        /// <summary>
        /// Time zone information.
        /// </summary>
        public TimeZoneInfo TimeZoneInfo { get; set; } = TimeZoneInfo.Local;

        /// <summary>
        /// Next run time.
        /// </summary>
        public DateTime? NextRunTime => _nextRunTime ?? CronExpression.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo)?.DateTime;

        public CronJob(string name, CronExpression cronExpression, Func<Task> action)
        {
            _action = action;
            Name = name;
            CronExpression = cronExpression;
        }

        public CronJob(string name, CronExpression cronExpression, Action action) 
            : this(name, cronExpression, () =>
            {
                action.Invoke();
                return Task.CompletedTask;
            })
        {
        }

        public CronJob(string name, string cronExpression, Action action) 
            : this(name, ParseCronExpression(cronExpression), action)
        {
        }

        public CronJob(string name, string cronExpression, Func<Task> action)
            : this(name, ParseCronExpression(cronExpression), action)
        {
        }

        private static CronExpression ParseCronExpression(string cronExpression)
        {
            return cronExpression.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length == 6
                ? CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds)
                : CronExpression.Parse(cronExpression);
        }

        private async Task InternalExecute()
        {
            if (AllowConcurrentExecution)
            {
                Interlocked.Increment(ref _runningFlag);
            }
            else
            {
                // Already running, skip execution.
                if (Interlocked.CompareExchange(ref _runningFlag, 1, 0) == 1)
                {
                    return;
                }
            }

            try
            {
                await _action.Invoke();
            }
            finally
            {
                if (AllowConcurrentExecution)
                {
                    Interlocked.Decrement(ref _runningFlag);
                }
                else
                {
                    Interlocked.Exchange(ref _runningFlag, 0);
                }
            }
        }

        internal void Execute(DateTime currentTime)
        {
            if (_nextRunTime.HasValue && AreTimesEqualToSecond(_nextRunTime.Value, currentTime))
            {
                Task.Run(InternalExecute);
            }

            if (_nextRunTime.HasValue == false || _nextRunTime.Value <= currentTime)
            {
                _nextRunTime = CronExpression.GetNextOccurrence(new DateTimeOffset(currentTime), TimeZoneInfo)?.DateTime;
            }
        }

        private static bool AreTimesEqualToSecond(DateTime dateTime1, DateTime dateTime2)
        {
            return dateTime1.Ticks / TimeSpan.TicksPerSecond == dateTime2.Ticks / TimeSpan.TicksPerSecond;
        }
    }
}
