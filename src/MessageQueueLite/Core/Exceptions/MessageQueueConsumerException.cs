using System;

namespace MessageQueueLite.Core
{
    public class MessageQueueConsumerException : MessageQueueException
    {
        public MessageQueueConsumerException()
        {
        }

        public MessageQueueConsumerException(string message) : base(message)
        {
        }

        public MessageQueueConsumerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
