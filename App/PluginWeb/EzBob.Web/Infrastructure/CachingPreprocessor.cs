using System;
using System.Web;
using System.Web.Caching;
using SquishIt.Framework;
using SquishIt.Framework.Base;

namespace EzBob.Web.Infrastructure
{
    /// <summary>
    /// A Caching preprocessor for squish it. Can be used as a proxy for any other preprocesser. Mainly for debug mode.
    /// </summary>
    /// <typeparam name="T">Any other preprocessor, which results should be cached.</typeparam>
    public class CachingPreprocessor<T> : Preprocessor where T: Preprocessor, new()
    {
        private readonly T _target;
        private readonly Cache _cache;
        private readonly string _preffix;

        public CachingPreprocessor()
        {
            _target = new T();
            _cache = HttpRuntime.Cache;
            _preffix = "CachingPreprocessor" + typeof(T).Name;
        }

        public override IProcessResult Process(string filePath, string content)
        {
            var key = GetKey(filePath, content);
            var processed = _cache.Get(key) as IProcessResult;
            
            if (processed != null) return processed;

            processed = _target.Process(filePath, content);
            _cache.Add(key, processed, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(8), CacheItemPriority.High, null);
            
            return processed;
        }

        private string GetKey(string filePath, string content)
        {
            return _preffix + filePath.GetHashCode() + content.GetHashCode();
        }

        public override string[] Extensions
        {
            get { return _target.Extensions; }
        }
    }
}