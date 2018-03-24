using System;

namespace JWadsworth.CloudWatchMiddleware
{
    /// <summary>
    /// Timer class that tracks elapsed time
    /// </summary>
    public class Timer
    {
        /// <summary>
        /// The time span.
        /// </summary>
        private TimeSpan timeSpan;

        /// <summary>
        /// Starts the timer (restarts it if it's already been started).
        /// </summary>
        public void Start()
        {
            timeSpan = TimeSpan.FromTicks(DateTimeOffset.UtcNow.Ticks);
        }

        /// <summary>
        /// Gets the elapsed milliseconds from when the timer started.
        /// </summary>
        /// <returns>The total number of milliseconds from when the timer started.</returns>
        public double GetElapsedMilliseconds()
        {
            return TimeSpan.FromTicks(DateTimeOffset.UtcNow.Ticks).Subtract(timeSpan).TotalMilliseconds;
        }
    }
}
