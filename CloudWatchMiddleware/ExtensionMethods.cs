using Microsoft.AspNetCore.Builder;

namespace JWadsworth.CloudWatchMiddleware
{

    /// <summary>
    /// Extension methods for the MetricMiddleware class.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Add MetricMiddleware to the application middleware.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>A builder.</returns>
        public static IApplicationBuilder UseCloudWatchMetrics(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MetricMiddleware>();
        }
    }
}
