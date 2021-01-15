using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Dapper.Extensions.Snapper.Helpers.Cache
{
    public class SnapperMemoryCache : ICache
    {
        public object Get(string key)
        {
            return MemoryCache.Default[key];
        }

        public T Get<T>(string key)
        {
            var cached = MemoryCache.Default[key];
            if (cached == null)
                return default;

            try
            {
                return (T)cached;
            }
            catch (InvalidCastException)
            {
                return default;
            }
        }

        /// <summary>
        /// Add an item to the cache. If a <paramref name="expirationInSeconds"/> is not specified then the cache will never expire.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The cashed object</param>
        /// <param name="expirationInSeconds">How many seconds it takes for the cache to expire. If not passed the cache will never expire.</param>
        public void Add(string key, object value, int expirationInSeconds = 0)
        {
            ObjectCache cache = MemoryCache.Default;

            CacheItemPolicy policy = new CacheItemPolicy()
            {
                AbsoluteExpiration = expirationInSeconds > 0 ? DateTimeOffset.Now.AddSeconds(expirationInSeconds) : ObjectCache.InfiniteAbsoluteExpiration
            };

            cache.Set(key, value, policy);
        }

        /// <summary>
        /// Invalidate all cached items
        /// </summary>
        public void Invalidate()
        {
            ObjectCache cache = MemoryCache.Default;
            List<string> cacheKeys = cache.Select(kvp => kvp.Key).ToList();
            foreach (string cacheKey in cacheKeys)
            {
                cache.Remove(cacheKey);
            }
        }

        /// <summary>
        /// Invalidate specific item(s) in the cache
        /// </summary>
        /// <param name="key"></param>
        public void Invalidate(params string[] cacheKeys)
        {
            ObjectCache cache = MemoryCache.Default;
            foreach (string cacheKey in cacheKeys)
            {
                cache.Remove(cacheKey);
            }
        }

        /// <summary>
        /// Invalidate items based on the start of their key
        /// </summary>
        /// <param name="cacheKeyPrefix"></param>
        public void InvalidateByPrefix(string cacheKeyPrefix)
        {
            ObjectCache cache = MemoryCache.Default;
            var cachedObjects = cache.Where(x => x.Key.StartsWith(cacheKeyPrefix));
            foreach (var cachedObj in cachedObjects)
            {
                cache.Remove(cachedObj.Key);
            }
        }

        public TReturn CacheExecution<TReturn>(Func<TReturn> toExecute, string cacheKey, bool useCache = true, int expirationInSeconds = 0)
        {
            if (useCache)
            {
                var cachedValue = (TReturn)Get(cacheKey);
                if (cachedValue == null)
                {
                    cachedValue = toExecute.Invoke();
                }
                Add(cacheKey, cachedValue, expirationInSeconds);
                return cachedValue;
            }

            return toExecute.Invoke();
        }

        public async Task<TReturn> CacheAsyncExecution<TReturn>(Func<Task<TReturn>> toExecute, string cacheKey, bool useCache = true, int expirationInSeconds = 0)
        {
            if (useCache)
            {
                var cachedValue = (TReturn)Get(cacheKey);
                if (cachedValue == null)
                {
                    cachedValue = await toExecute.Invoke();
                }
                Add(cacheKey, cachedValue, expirationInSeconds);
                return cachedValue;
            }

            return await toExecute.Invoke();
        }
    }
}
