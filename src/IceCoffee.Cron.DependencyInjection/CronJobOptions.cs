using System.ComponentModel.DataAnnotations;

namespace IceCoffee.Cron.DependencyInjection
{
    public class CronJobOptions
    {
        /// <summary>
        /// Cron expression.
        /// </summary>
        [Required]
        public string CronExpression { get; set; } = null!;

        /// <summary>
        /// Whether the job allows concurrent execution.
        /// </summary>
        public bool AllowConcurrentExecution { get; set; } = true;

        /// <summary>
        /// Time zone.
        /// </summary>
        public string? TimeZone { get; set; }

        /// <summary>
        /// Whether the job is enabled.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Whether the job is run once at start.
        /// </summary>
        public bool RunOnceAtStart {  get; set; }
    }
}
