namespace MessageQueueLite.Core.Models
{
    public class EnqueueResponse
    {
        public string MessageId { get; }

        public EnqueueResponse(string messageId)
        {
            MessageId = messageId;
        }
    }
}
