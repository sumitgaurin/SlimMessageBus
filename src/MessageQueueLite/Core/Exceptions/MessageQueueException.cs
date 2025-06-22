using System;

namespace MessageQueueLite.Core
{
    public class MessageQueueException : Exception
    {
        public MessageQueueException()
        {
        }

        public MessageQueueException(string message) : base(message)
        {
        }

        public MessageQueueException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

}
