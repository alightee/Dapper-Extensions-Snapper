using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extensions.Snapper.Helpers.Cache
{
    public interface ICache
    {
        /// <summary>
        /// Get a cached object by key. Returns null if there is no cached object asociated with the specific key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object Get(string key);

        /// <summary>
        /// Get a cached object by key. Returns null if there is no cached object asociated with the specific key or if the object stored can't be cast to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T Get<T>(string key);

        /// <summary>
        /// Add an item to the cache. If a <paramref name="expirationInSeconds"/> is not specified then the cache will never expire.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The cashed object</param>
        /// <param name="expirationInSeconds">How many seconds it takes for the cache to expire. If not passed the cache will never expire.</param>
        void Add(string key, object value, int expirationInSeconds = 0);

        /// <summary>
        /// Invalidate all cached items
        /// </summary>
        void Invalidate();

        /// <summary>
        /// Invalidate specific item(s) in the cache
        /// </summary>
        /// <param name="key"></param>
        void Invalidate(params string[] cacheKeys);

        /// <summary>
        /// Invalidate items based on the start of their key
        /// </summary>
        /// <param name="cacheKeyPrefix"></param>
        void InvalidateByPrefix(string cacheKeyPrefix);

        /// <summary>
        /// Helper function that automatically checks cache before executing a piece of code.
        /// <para>If there is a value cached then it won't execute the piece of code</para>
        /// <para>If there's no cached value / the cache expired then it executes the code and caches the return value for future use</para>
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="toExecute">The piece of code to execute</param>
        /// <param name="cacheKey">The cache key used to retrieve the cached value</param>
        /// <param name="useCache">If this is false then the code will just execute normally without any caching</param>
        /// <param name="expirationInSeconds">How much time (in seconds) it takes for the cached value to expire</param>
        /// <returns></returns>
        TReturn CacheExecution<TReturn>(Func<TReturn> toExecute, string cacheKey, bool useCache = true, int expirationInSeconds = 0);

        /// <summary>
        /// Async version of <see cref="CacheExecution"/>
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="toExecute">The piece of code to execute</param>
        /// <param name="cacheKey">The cache key used to retrieve the cached value</param>
        /// <param name="useCache">If this is false then the code will just execute normally without any caching</param>
        /// <param name="expirationInSeconds">How much time (in seconds) it takes for the cached value to expire</param>
        /// <returns></returns>
        Task<TReturn> CacheAsyncExecution<TReturn>(Func<Task<TReturn>> toExecute, string cacheKey, bool useCache = true, int expirationInSeconds = 0);
    }
}
