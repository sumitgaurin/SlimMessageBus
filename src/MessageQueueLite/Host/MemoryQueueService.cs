using MessageQueueLite.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MessageQueueLite.Host
{
    public class MemoryQueueService
    {
        public string ServiceType { get; } = "Memory";

        public MemoryQueueService(MemoryQueueServiceSettings serviceSettings,
            IDictionary<string, object>? defaultHeaders = null)
        {

        }

        public async Task CreateQueue(string queueName,
            MemoryQueueSettings? queueSettings = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteQueue(string queueName,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<MessageQueueStatus> GetQueueStatus(string queueName,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }        
    }
}
