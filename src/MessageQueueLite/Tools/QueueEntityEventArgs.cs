using System;

namespace MessageQueueLite.Tools
{
    internal class QueueEntityEventArgs : EventArgs
    {
        public ulong DeliveryTag { get; set; } = 0;

        public uint RetryCount { get; set; } = 0; 

        public bool IsAcknowledged { get; set; }

        public byte[] Message { get; set; } = Array.Empty<byte>();
    }
}
