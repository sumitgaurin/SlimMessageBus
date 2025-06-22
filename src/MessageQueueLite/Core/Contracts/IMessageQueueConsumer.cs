using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueueLite.Core.Contracts
{
    public interface IMessageQueueConsumer : IDisposable
    {
        Task<TMessage[]> Dequeue<TMessage>(int batchCount,
            string? path = null,
            IDictionary<string, object>? headers = null,
            CancellationToken cancellationToken = default) where TMessage : class;

        Task Acknowledge<TMessage>(TMessage message,
            string? path = null,
            IDictionary<string, object>? headers = null,
            CancellationToken cancellationToken = default) where TMessage : class;
    }
}
