using ILGPU.Runtime;

namespace DataScience.Core
{
    public interface ICacheable
    {
        public uint Id { get; set; }
        public uint Cache();
        public uint UpdateCache();
        public void Dispose();
        public long MemorySize();
        public MemoryBuffer Allocate();


    }

}
