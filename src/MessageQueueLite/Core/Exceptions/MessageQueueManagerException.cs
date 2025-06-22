using System;
using System.Collections.Generic;
using System.Text;

namespace MessageQueueLite.Core.Exceptions
{
    public class MessageQueueManagerException : MessageQueueException
    {
        public MessageQueueManagerException()
        {
        }

        public MessageQueueManagerException(string message) : base(message)
        {
        }

        public MessageQueueManagerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
