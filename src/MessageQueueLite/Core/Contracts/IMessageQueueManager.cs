using System;
using System.Threading.Tasks;

namespace MessageQueueLite.Core.Contracts
{
    public interface IMessageQueueManager : IDisposable
    {
        Task<bool> CreateQueue(string path);

        Task<bool> DeleteQueue(string path);

        Task<int> GetMessageCount(string path);

        Task<int> GetUnacknowledgedMessageCount(string path);
    }
}
