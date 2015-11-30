// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using StackExchange.Redis;

namespace MultiTenantSurveyApp.TokenStorage
{
    // sample implementation of a token cache which persists tokens specific to a user to redis to be used in multi tenanted scenarios
    // the key is the users object unique object identifier   
    // [masimms-roshar] Since we expect this to act as a copy-paste sample, please add an optional verbose log for 
    // all access to ease future debugging
    //[manikris] done
    public class RedisTokenCache : TokenCache
    {
        private IDatabase _cache;
        private TokenCacheKey _key;
        private ILogger _logger;

        public async static Task<RedisTokenCache> CreateCacheAsync(IConnectionMultiplexer connection, TokenCacheKey key, ILogger logger)
        {
            var cache = connection.GetDatabase();
            byte[] cachedData = null;
            try
            {
                cachedData = await cache.StringGetAsync(key.ToString()).ConfigureAwait(false);
                logger.TokensRetrievedFromStore(key.ToString());
            }
            catch (Exception exp)
            {
                logger.ReadFromCacheFailed(exp);
                throw;
            }

            return new RedisTokenCache(cache, key, logger, cachedData);
        }

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

