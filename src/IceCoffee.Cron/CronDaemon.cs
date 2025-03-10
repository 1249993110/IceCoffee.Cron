﻿using System.Collections.Concurrent;
using System.Timers;

namespace IceCoffee.Cron
{
    public class CronDaemon : ICronDaemon
    {
        private readonly System.Timers.Timer _timer = new System.Timers.Timer(500);
        private readonly ConcurrentDictionary<string, CronJob> _cronJobs = new();
        private DateTime _lastRunTime;

        public IReadOnlyDictionary<string, CronJob> CronJobs => _cronJobs;

        private static readonly Lazy<CronDaemon> _default = new(true);
        public static CronDaemon Default => _default.Value;

        public CronDaemon()
        {
            _timer.AutoReset = true;
            _timer.Elapsed += OnTimer_Elapsed;
        }

        public void AddJob(CronJob cronJob)
        {
            _cronJobs[cronJob.Name] = cronJob;
        }

        public bool RemoveJob(CronJob cronJob)
        {
            return _cronJobs.TryRemove(cronJob.Name, out _);
        }

        public bool RemoveJob(string name)
        {
            return _cronJobs.TryRemove(name, out _);
        }

        public void Start()
        {
            if (_timer.Enabled)
            {
                throw new InvalidOperationException("The daemon is already running.");
            }

            _lastRunTime = DateTime.Now;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Clear()
        {
            _cronJobs.Clear();
        }

        private void OnTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            DateTime now = DateTime.Now;
            if (now.Second != _lastRunTime.Second)
            {
                _lastRunTime = now;
                //Parallel.ForEach(_cronJobs.Values, job => job.Execute(now, _cts.Token));
                foreach (var job in _cronJobs.Values)
                {
                    job.Execute(now);
                }
            }
        }
    }
}
