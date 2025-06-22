using System;

namespace MessageQueueLite.Core.Contracts
{
    public interface IMessageQueue : IAsyncDisposable, IMessageQueueConsumer, IMessageQueueProducer
    {
    }
}
