using MessageQueueLite.Core;
using MessageQueueLite.Core.Serialization;

namespace MessageQueueLite.Host
{
    public class MemoryQueueSettings
    {
        /// <summary>
        /// Gets or sets the elapsed seconds after which unacknowledged messeges are re-queued..
        /// </summary>
        /// <value>
        /// The meassage lease interval in seconds.
        /// </value>
        public uint MeassageLeaseIntervalInSeconds { get; set; } = 1800;

        /// <summary>
        /// Gets or sets the monitoring interval for lease expiration in seconds.
        /// </summary>
        /// <value>
        /// The lease monitoring interval in seconds.
        /// </value>
        public uint LeaseMonitoringIntervalInSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the maximum number of times an lease expired message is retried.
        /// </summary>
        /// <value>
        /// The message retry count.
        /// </value>
        public uint MaxRetryCount { get; set; } = 3;
    }
}
