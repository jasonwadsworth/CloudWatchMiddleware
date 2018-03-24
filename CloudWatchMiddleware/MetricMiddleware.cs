using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace JWadsworth.CloudWatchMiddleware
{
    /// <summary>
    /// AWS CloudWatch Metrics Middleware
    /// </summary>
    public class MetricMiddleware
    {
        /// <summary>
        /// The semaphore for locking.
        /// </summary>
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

        /// <summary>
        /// The next request delegate.
        /// </summary>
        private readonly RequestDelegate next;

        /// <summary>
        /// The CloudWatch interface.
        /// </summary>
        private readonly Amazon.CloudWatch.IAmazonCloudWatch cloudWatch;

        /// <summary>
        /// The configuration.
        /// </summary>
        private readonly MetricMiddlewareConfiguration configuration;

        /// <summary>
        /// The data to send.
        /// </summary>
        private readonly List<Amazon.CloudWatch.Model.MetricDatum> data = new List<Amazon.CloudWatch.Model.MetricDatum>();

        /// <summary>
        /// The last send time.
        /// </summary>
        private DateTimeOffset lastSend = DateTimeOffset.UtcNow;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next request delegate.</param>
        /// <param name="cloudWatch">The cloud watch.</param>
        /// <param name="configuration">The configuration.</param>
        public MetricMiddleware(RequestDelegate next, Amazon.CloudWatch.IAmazonCloudWatch cloudWatch, MetricMiddlewareConfiguration configuration)
        {
            this.next = next;
            this.cloudWatch = cloudWatch;
            this.configuration = configuration;
        }

        /// <summary>
        /// Invokes the specified context.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="timer">The timer. The timer should be a scoped value so that it is the same timer used by the StartTimerMiddleware.</param>
        /// <returns>
        /// A task.
        /// </returns>
        public async Task Invoke(HttpContext context)
        {
            TimeSpan timeSpan = TimeSpan.FromTicks(DateTimeOffset.UtcNow.Ticks);

            await next(context);

            try
            {
                await semaphore.WaitAsync();

                data.Add(new Amazon.CloudWatch.Model.MetricDatum
                {
                    Dimensions = new System.Collections.Generic.List<Amazon.CloudWatch.Model.Dimension>
                        {
                            new Amazon.CloudWatch.Model.Dimension
                            {
                                Name = "StatusCode",
                                Value = context.Response.StatusCode.ToString()
                            },
                            new Amazon.CloudWatch.Model.Dimension
                            {
                                Name = "Path",
                                Value = context.Request.Path
                            }
                        },
                    Unit = Amazon.CloudWatch.StandardUnit.Milliseconds,
                    StorageResolution = configuration.StorageResolution,
                    Value = TimeSpan.FromTicks(DateTimeOffset.UtcNow.Ticks).Subtract(timeSpan).TotalMilliseconds,
                    MetricName = "ResponseTime"
                });
            }
            finally
            {
                semaphore.Release();
            }

            // send if we have 20 items or if it's been over 5 seconds
            if (data.Count >= 20 || lastSend < DateTimeOffset.UtcNow.AddSeconds(-5))
            {
                await SendDataAsync();
            }
        }

        /// <summary>
        /// Sends the data asynchronously.
        /// </summary>
        /// <returns>A task.</returns>
        private async Task SendDataAsync()
        {
            // use a copy of the data so we can limit the time in the blocking state
            Amazon.CloudWatch.Model.MetricDatum[] copy = null;

            try
            {
                await semaphore.WaitAsync();

                copy = new Amazon.CloudWatch.Model.MetricDatum[data.Count];
                data.CopyTo(copy);
                data.Clear();
                lastSend = DateTimeOffset.UtcNow;
            }
            finally
            {
                semaphore.Release();
            }

            var t = Task.Factory.StartNew(() =>
            {
                // send the metrics to AWS CloudWatch
                // NOTE: for now this is fire and forget
                cloudWatch.PutMetricDataAsync(new Amazon.CloudWatch.Model.PutMetricDataRequest
                {
                    MetricData = copy.ToList(),
                    Namespace = configuration.MetricNamespace
                });
            }, TaskCreationOptions.LongRunning);
        }
    }
}
