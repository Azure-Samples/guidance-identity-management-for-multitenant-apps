// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using StackExchange.Redis;
using MultiTenantSurveyApp.Common;

namespace MultiTenantSurveyApp.TokenStorage
{
    // sample implementation of a token cache which persists tokens specific to a user to redis to be used in multi tenanted scenarios
    public class RedisTokenCache : TokenCache
    {
        private IConnectionMultiplexer _connection;
        private IDatabase _cache;
        private TokenCacheKey _key;
        private ILogger _logger;
        /// <summary>
        /// This factory method loads up the dictionary in the base TokenCache class with the tokens read from redis.
        /// Read http://www.cloudidentity.com/blog/2014/07/09/the-new-token-cache-in-adal-v2/ for more details on writing a custom token cache.
        /// The post above explains why we need to load up the base class token cache dictionary as soon as the cache is created.
        /// We want to do this asynchronously and this requires the factory method.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="key"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public async static Task<RedisTokenCache> CreateCacheAsync(IConnectionMultiplexer connection, TokenCacheKey key, ILogger logger)
        {
            Guard.ArgumentNotNull(connection, "IConnectionMultiplexer");
            Guard.ArgumentNotNull(key, "key");
            Guard.ArgumentNotNull(logger, "logger");

            var redisTokenCache = new RedisTokenCache();
            await redisTokenCache.InitializeAsync(connection, key, logger).ConfigureAwait(false);
            return redisTokenCache;
        }
        private RedisTokenCache() : base() { }

        private async Task InitializeAsync(IConnectionMultiplexer connection, TokenCacheKey key, ILogger logger)
        {
            _connection = connection;
            _key = key;
            _logger = logger;
            _cache = _connection.GetDatabase();
            AfterAccess = AfterAccessNotification;
            await LoadFromStoreAsync().ConfigureAwait(false);
        }

        private async Task LoadFromStoreAsync()
        {
            try
            {
                byte[] cachedData = await _cache.StringGetAsync(_key.ToString()).ConfigureAwait(false);
                this.Deserialize((cachedData == null) ? null : cachedData);
                _logger.TokensRetrievedFromStore(this._key.ToString());
            }
            catch (Exception exp)
            {
                _logger.ReadFromCacheFailed(exp);
                throw;
            }
        }

        // Triggered right after ADAL accessed the cache.
        public async void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (this.HasStateChanged)
            {
                try
                {
                    await _cache.StringSetAsync(this._key.ToString(), this.Serialize()).ConfigureAwait(false);
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

        // clean the cache of all tokens associated with the user.
        public override void Clear()
        {
            base.Clear();
            _cache.KeyDelete(_key.ToString());
            _logger.TokenCacheCleared(_key.ToString());
        }
    }
}

