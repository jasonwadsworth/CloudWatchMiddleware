namespace JWadsworth.CloudWatchMiddleware
{
    /// <summary>
    /// Configuration for MetricMiddleware.
    /// </summary>
    public class MetricMiddlewareConfiguration
    {
        /// <summary>
        /// Gets or sets the metric namespace.
        /// <para>
        /// This value will be the namespace used in the metrics sent to CloudWatch.
        /// From AWS Documentation:
        ///     You cannot specify a namespace that begins with "AWS/". Namespaces that begin
        ///     with "AWS/" are reserved for use by Amazon Web Services products.
        /// </para>
        /// </summary>
        public string MetricNamespace { get; set; } = "/";

        /// <summary>
        /// Gets or sets the storage resolution.
        /// <para>
        /// This value will be used on the metric data sent to CloudWatch. 
        /// From AWS Documentation:
        ///     Valid values are 1 and 60. Setting this to 1 specifies this metric as a high-resolution
        ///     metric, so that CloudWatch stores the metric with sub-minute resolution down
        ///     to one second. Setting this to 60 specifies this metric as a regular-resolution
        ///     metric, which CloudWatch stores at 1-minute resolution. Currently, high resolution
        ///     is available only for custom metrics. For more information about high-resolution
        ///     metrics, see High-Resolution Metrics in the Amazon CloudWatch User Guide.
        ///     This field is optional, if you do not specify it the default of 60 is used.
        /// </para>
        /// </summary>
        public int StorageResolution { get; set; } = 60;
    }
}
