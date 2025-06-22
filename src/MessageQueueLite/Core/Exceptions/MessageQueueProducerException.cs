using System;

namespace MessageQueueLite.Core.Exceptions
{
    public class MessageQueueProducerException : MessageQueueException
    {
        public MessageQueueProducerException()
        {
        }

        public MessageQueueProducerException(string message) : base(message)
        {
        }

        public MessageQueueProducerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
