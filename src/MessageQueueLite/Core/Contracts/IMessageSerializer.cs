using System;

namespace MessageQueueLite.Core
{
    public interface IMessageSerializer
    {
        object Deserialize(Type t, byte[] payload);

        byte[] Serialize(Type t, object message);
    }
}
