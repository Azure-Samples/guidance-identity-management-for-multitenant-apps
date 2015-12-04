// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

#if DNX451
using Microsoft.Extensions.DependencyInjection;
using Tailspin.Surveys.TokenStorage;
using StackExchange.Redis;
#endif

namespace Tailspin.Surveys.Web.Security
{
    /// <summary>
    /// Returns and manages the instance of token cache to be used when making use of ADAL. 
    /// This returns the default token cache in the case of DNX core and Redis token cache when using DNX451 since Redis client is supported only in DNX451
    /// Lifetime should be scoped- we need a new instance for every request if we are using the redis cache
    public class TokenCacheService : ITokenCacheService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        private TokenCache cache = null;

        /// <summary>
        /// Creates a new instance of <see cref="Tailspin.Surveys.Security.TokenCacheService"/>
        /// </summary>
        /// <param name="contextAccessor"><see cref="Microsoft.AspNet.Http.IHttpContextAccessor"/> used to access the current <see cref="Microsoft.AspNet.Http.HttpContext"/></param>
        /// <param name="loggerFactory"><see cref="Microsoft.Extensions.Logging.ILoggerFactory"/> used to create type-specific <see cref="Microsoft.Extensions.Logging.ILogger"/> instances.</param>
        public TokenCacheService(IHttpContextAccessor contextAccessor, ILoggerFactory loggerFactory)
        {
            _httpContextAccessor = contextAccessor;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<TokenCacheService>();
        }

        /// <summary>
        /// Returns an instance of <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCache"/>.
        /// </summary>
        /// <param name="userObjectId">Azure Active Directory user's ObjectIdentifier.</param>
        /// <param name="clientId">Azure Active Directory ApplicationId.</param>
        /// <returns>An instance of <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCache"/>.</returns>
        public async Task<TokenCache> GetCacheAsync(string userObjectId, string clientId)
        {
            if (cache != null)
            {
                return cache;
            }
#if DNXCORE50
            // we are falling back to the default cache for DNXCORE only because we cannot use the redis one here at present due to lack of DNX core support for stackexchange lib
            await Task.CompletedTask;
            return (cache=TokenCache.DefaultShared);
#else
            var key = new TokenCacheKey(userObjectId, clientId);
            /// StackExchange.Redis recommends creating only a single connection. We choose to create connection outside and pass it in because:
            /// 1. We want the consumer to pass the connection since the connection could be potentially used for other cache operations.
            /// 2. There are many configuration settings for the connection and we want to leave it open to the user to choose whats appropriate.
            /// 3. Testability if we allow injecting the connection
            var connection = _httpContextAccessor.HttpContext.ApplicationServices.GetService<IConnectionMultiplexer>();
            return (cache = await RedisTokenCache.CreateCacheAsync(connection, key, _loggerFactory.CreateLogger<RedisTokenCache>()));
#endif
        }

        /// <summary>
        /// Clears the token cache.
        /// </summary>
        /// <param name="userObjectId">Azure Active Directory user's ObjectIdentifier.</param>
        /// <param name="clientId">Azure Active Directory ApplicationId.</param>
        public async Task ClearCacheAsync(string userObjectId, string clientId)
        {
            var cache = await GetCacheAsync(userObjectId, clientId);
            var items = cache.ReadItems().Where(ti => ti.UniqueId == userObjectId && ti.ClientId == clientId);
            foreach (var ti in items)
            {
                cache.DeleteItem(ti);
                //Note: This will trigger the writes to redis if we are using the redis implementation 
            }
        }
    }
}
