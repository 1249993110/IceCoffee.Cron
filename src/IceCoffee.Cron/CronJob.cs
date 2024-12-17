using Cronos;

namespace IceCoffee.Cron
{
    public class CronJob : ICronJob
    {
        private readonly Func<Task> _action;
        private DateTime? _nextRunTime;
        private volatile int _runningFlag;

        public string Name { get; private set; }
        public CronExpression CronExpression { get; private set; }
        public bool IsRunning => _runningFlag > 0;
        public bool AllowConcurrentExecution { get; private set; }

        public CronJob(string name, CronExpression cronExpression, Func<Task> action, bool allowConcurrentExecution = true)
        {
            _action = action;
            Name = name;
            CronExpression = cronExpression;
            AllowConcurrentExecution = allowConcurrentExecution;
        }

        public CronJob(string name, CronExpression cronExpression, Action action, bool allowConcurrentExecution = true) 
            : this(name, cronExpression, () =>
            {
                action.Invoke();
                return Task.CompletedTask;
            }, allowConcurrentExecution)
        {
        }

        public CronJob(string name, string cronExpression, Action action, bool allowConcurrentExecution = true) 
            : this(name, ParseCronExpression(cronExpression), action, allowConcurrentExecution)
        {
        }

        public CronJob(string name, string cronExpression, Func<Task> action, bool allowConcurrentExecution = true)
            : this(name, ParseCronExpression(cronExpression), action, allowConcurrentExecution)
        {
        }

        private static CronExpression ParseCronExpression(string cronExpression)
        {
            return cronExpression.Split(' ').Length >= 6
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

        public virtual void Execute(DateTime currentTime)
        {
            if (_nextRunTime.HasValue && AreTimesEqualToSecond(_nextRunTime.Value, currentTime))
            {
                Task.Run(InternalExecute);
            }

            if (_nextRunTime.HasValue == false || _nextRunTime.Value <= currentTime)
            {
                _nextRunTime = CronExpression.GetNextOccurrence(new DateTimeOffset(currentTime), TimeZoneInfo.Local)?.DateTime;
            }
        }

        private static bool AreTimesEqualToSecond(DateTime dateTime1, DateTime dateTime2)
        {
            return dateTime1.Ticks / TimeSpan.TicksPerSecond == dateTime2.Ticks / TimeSpan.TicksPerSecond;
        }
    }
}
