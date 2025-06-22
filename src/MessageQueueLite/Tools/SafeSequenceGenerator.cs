namespace MessageQueueLite.Tools
{
    internal class SafeSequenceGenerator
    {
        private ulong sequence;

        public SafeSequenceGenerator() : this(0)
        {
        }

        public SafeSequenceGenerator(ulong seed)
        {
            sequence = seed;
        }

        public ulong GetNext()
        {
            return ++sequence;
        }
    }
}
