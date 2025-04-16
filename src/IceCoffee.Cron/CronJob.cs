using Cronos;

namespace IceCoffee.Cron
{
    public class CronJob
    {
        private readonly Func<Task> _action;
        private DateTime? _nextRunTime;
        private volatile int _runningFlag;

        /// <summary>
        /// Job action.
        /// </summary>
        public Func<Task> Action => _action;

        /// <summary>
        /// Whether the job is running.
        /// </summary>
        public bool IsRunning => _runningFlag != 0;

        /// <summary>
        /// Job name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Cron expression.
        /// </summary>
        public CronExpression CronExpression { get; private set; }

        /// <summary>
        /// Time zone information.
        /// </summary>
        public TimeZoneInfo TimeZoneInfo { get; private set; }

        /// <summary>
        /// Whether the job allows concurrent execution.
        /// </summary>
        public bool AllowConcurrentExecution { get; private set; }

        /// <summary>
        /// Next run time.
        /// </summary>
        public DateTime? NextRunTime => _nextRunTime ?? CronExpression.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo)?.DateTime;

        public CronJob(string name, CronExpression cronExpression, Func<Task> action, TimeZoneInfo? timeZoneInfo = null, bool allowConcurrentExecution = true)
        {
            _action = action;
            Name = name;
            CronExpression = cronExpression;
            TimeZoneInfo = timeZoneInfo ?? TimeZoneInfo.Local;
            AllowConcurrentExecution = allowConcurrentExecution;
        }

        public CronJob(string name, CronExpression cronExpression, Action action, TimeZoneInfo? timeZoneInfo = null, bool allowConcurrentExecution = true)
            : this(name, cronExpression, () =>
            {
                action.Invoke();
                return Task.CompletedTask;
            }, timeZoneInfo, allowConcurrentExecution)
        {
        }

        public CronJob(string name, string cronExpression, Action action, TimeZoneInfo? timeZoneInfo = null, bool allowConcurrentExecution = true) 
            : this(name, ParseCronExpression(cronExpression), action, timeZoneInfo, allowConcurrentExecution)
        {
        }

        public CronJob(string name, string cronExpression, Func<Task> action, TimeZoneInfo? timeZoneInfo = null, bool allowConcurrentExecution = true)
            : this(name, ParseCronExpression(cronExpression), action, timeZoneInfo, allowConcurrentExecution)
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
                if (Interlocked.CompareExchange(ref _runningFlag, 1, 0) != 0)
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
