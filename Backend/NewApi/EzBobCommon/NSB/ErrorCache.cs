namespace EzBobCommon.NSB
{
    using System.Runtime.Caching;

    public class ErrorCache : MemoryCache
    {
        public ErrorCache()
            : base("errors") { }
    }
}
