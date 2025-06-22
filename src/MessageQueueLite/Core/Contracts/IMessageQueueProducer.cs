using MessageQueueLite.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueueLite.Core.Contracts
{
    public interface IMessageQueueProducer : IDisposable
    {
        Task<EnqueueResponse> Enqueue<TMessage>(TMessage message,
            string? path = null,
            IDictionary<string, object>? headers = null,
            CancellationToken? cancellationToken = default) where TMessage : class;
    }
}
