namespace MessageQueueLite.Core
{
    public class MessageEnvelop<TMessage>
    {
        public ulong DeliveryTag { get; }

        public TMessage Message { get; }

        public MessageEnvelop(ulong deliveryTag, TMessage message)
        {
            DeliveryTag = deliveryTag;
            Message = message;
        }
    }
}
