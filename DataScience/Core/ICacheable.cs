namespace DataScience.Core
{
    public interface ICacheable
    {
        public uint LiveCount { get; }
        public uint ID { get; set; }
        public long MemorySize { get; set; }

        public uint Cache();
        public bool TryDeCache();

        public void IncrementLiveCount();

        public void DecrementLiveCount();

    }


}
