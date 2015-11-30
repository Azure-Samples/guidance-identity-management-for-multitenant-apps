// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using StackExchange.Redis;

namespace MultiTenantSurveyApp.TokenStorage
{
    /// <summary>
    /// Sample implementation of a token cache which persists tokens specific to a user to Redis to be used in multi-tenanted scenarios
    /// the key is the users object unique object identifier.
    /// </summary>
    public class RedisTokenCache : TokenCache
    {
        private IDatabase _cache;
        private TokenCacheKey _key;
        private ILogger _logger;

<<<<<<< HEAD
        /// <summary>
        /// Factory method to create instances of <see cref="MultiTenantSurveyApp.TokenStorage.RedisTokenCache"/>.  The constructor
        /// is intentionally made private.  Since we need to do some async initialization, the instance of the class is not ready
        /// for use at the completion of the constructor.
        /// </summary>
        /// <param name="connection">An instance of <see cref="StackExchange.Redis.IConnectionMultiplexer"/> to use for a connection to Redis.</param>
        /// <param name="key">An instance of <see cref="MultiTenantSurveyApp.TokenStorage.TokenCacheKey"/> containing the key for the new cache.</param>
        /// <param name="logger">An instance of <see cref="Microsoft.Extensions.Logging.ILogger"/> to use for logging.</param>
        /// <returns>An initialized instance of <see cref="MultiTenantSurveyApp.TokenStorage.TokenCacheKey"/>.</returns>
        public async static Task<RedisTokenCache> CreateCacheAsync(IConnectionMultiplexer connection, TokenCacheKey key, ILogger logger)
        {
            var cache = new RedisTokenCache(connection, key, logger);
            await cache.InitializeAsync();
            return cache;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="MultiTenantSurveyApp.TokenStorage.RedisTokenCache"/>.
        /// </summary>
        /// <param name="connection">An instance of <see cref="StackExchange.Redis.IConnectionMultiplexer"/> to use for a connection to Redis.</param>
        /// <param name="key">An instance of <see cref="MultiTenantSurveyApp.TokenStorage.TokenCacheKey"/> containing the key for this instance of cache.</param>
        /// <param name="logger">An instance of <see cref="Microsoft.Extensions.Logging.ILogger"/> to use for logging.</param>
        private RedisTokenCache(IConnectionMultiplexer connection, TokenCacheKey key, ILogger logger)
            : base()
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _cache = connection.GetDatabase();
            _key = key;
            _logger = logger;
            AfterAccess = AfterAccessNotification;
        }

        /// <summary>
        /// Initializes the cache data from Redis.  This is a separate method since we cannot have async methods in a constructor.
        /// </summary>
        /// <returns></returns>
        private async Task InitializeAsync()
        {
            try
            {
                byte[] cachedData = await _cache.StringGetAsync(_key.ToString())
                    .ConfigureAwait(false);
                this.Deserialize(cachedData);
                _logger.TokensRetrievedFromStore(_key.ToString());
=======
        public async static Task<RedisTokenCache> CreateCacheAsync(IConnectionMultiplexer connection, TokenCacheKey key, ILogger logger)
        {
            var cache = connection.GetDatabase();
            byte[] cachedData = null;
            try
            {
                cachedData = await cache.StringGetAsync(key.ToString()).ConfigureAwait(false);
                logger.TokensRetrievedFromStore(key.ToString());
>>>>>>> Changed configuration for redis
            }
            catch (Exception exp)
            {
                logger.ReadFromCacheFailed(exp);
                throw;
            }

            return new RedisTokenCache(cache, key, logger, cachedData);
        }

<<<<<<< HEAD
        /// <summary>
        /// Handles the AfterAccessNotification event, which is triggered right after ADAL accesses the cache.
        /// </summary>
        /// <param name="args">An instance of <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCacheNotificationArgs"/> containing information for this event.</param>
=======
        private RedisTokenCache(IDatabase database, TokenCacheKey key, ILogger logger, byte[] cachedData)
            : base()
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _cache = database;
            _key = key;
            _logger = logger;
            AfterAccess = AfterAccessNotification;
            Deserialize(cachedData);
        }

        // Triggered right after ADAL accessed the cache.
>>>>>>> Changed configuration for redis
        public async void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (this.HasStateChanged)
            {
                try
                {
                    await _cache.StringSetAsync(this._key.ToString(), this.Serialize())
                        .ConfigureAwait(false);
                    _logger.TokensWrittenToStore(args.ClientId, args.UniqueId, args.Resource);
                    this.HasStateChanged = false;
                }
                catch (Exception exp)
                {
                    _logger.WriteToRedisCacheFailed(exp);
                    throw;
                }
            }
        }

        /// <summary>
        /// Clears the cache of all tokens for the user.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            _cache.KeyDelete(_key.ToString());
            _logger.TokenCacheCleared(_key.ToString());
        }
    }
}

