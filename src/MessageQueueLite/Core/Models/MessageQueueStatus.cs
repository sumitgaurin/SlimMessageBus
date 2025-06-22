using System;

namespace MessageQueueLite.Core.Models
{
    public class MessageQueueStatus
    {
        public string QueueName { get; set; } = string.Empty;

        public int QueueLength { get; set; } = 0;

        public int PendingAckCount { get; set; } = 0;

        public MessageQueueStatus(string queueName)
        {
            QueueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
        }
    }
}