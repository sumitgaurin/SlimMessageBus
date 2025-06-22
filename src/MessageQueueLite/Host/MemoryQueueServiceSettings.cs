using MessageQueueLite.Core;
using MessageQueueLite.Core.Serialization;

namespace MessageQueueLite.Host
{
    public class MemoryQueueServiceSettings : MemoryQueueSettings
    {
        /// <summary>
        /// Gets or sets the serializer.
        /// </summary>
        /// <value>
        /// The serializer.
        /// </value>
        public IMessageSerializer Serializer { get; set; } = new JsonMessageSerializer();
    }
}
