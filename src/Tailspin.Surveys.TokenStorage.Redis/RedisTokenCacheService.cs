using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using StackExchange.Redis;
using Microsoft.Extensions.DependencyInjection;
using Tailspin.Surveys.Common;

namespace Tailspin.Surveys.TokenStorage.Redis
{
    /// <summary>
    /// Returns an instance of the RedisTokenCache 
    /// </summary>
    public class RedisTokenCacheService : TokenCacheService
    {
        private IConnectionMultiplexer _connection;

        /// <summary>
        /// Creates a new instance of <see cref="Tailspin.Surveys.Security.TokenCacheService"/>
        /// </summary>
        /// <param name="connection"><see cref="StackExchange.Redis.IConnectionMultiplexer"/> used to access Redis.</param>
        /// <param name="contextAccessor"><see cref="Microsoft.AspNet.Http.IHttpContextAccessor"/> used to access the current <see cref="Microsoft.AspNet.Http.HttpContext"/></param>
        /// <param name="loggerFactory"><see cref="Microsoft.Extensions.Logging.ILoggerFactory"/> used to create type-specific <see cref="Microsoft.Extensions.Logging.ILogger"/> instances.</param>
        public RedisTokenCacheService(IConnectionMultiplexer connection, IHttpContextAccessor contextAccessor, ILoggerFactory loggerFactory)
            : base(contextAccessor, loggerFactory)
        {
            Guard.ArgumentNotNull(connection, nameof(connection));
            _connection = connection;
        }

        /// <summary>
        /// Returns an instance of <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCache"/>.
        /// </summary>
        /// <param name="userObjectId">Azure Active Directory user's ObjectIdentifier.</param>
        /// <param name="clientId">Azure Active Directory ApplicationId.</param>
        /// <returns>An instance of <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCache"/>.</returns>
        public override async Task<TokenCache> GetCacheAsync(string userObjectId, string clientId)
        {
            if (_cache == null)
            {
                var key = new TokenCacheKey(userObjectId, clientId);
                /// StackExchange.Redis recommends creating only a single connection. We choose to create connection outside and pass it in because:
                /// 1. We want the consumer to pass the connection since the connection could be potentially used for other cache operations.
                /// 2. There are many configuration settings for the connection and we want to leave it open to the user to choose whats appropriate.
                /// 3. Testability if we allow injecting the connection
                _cache = await RedisTokenCache.CreateCacheAsync(_connection, key, _loggerFactory);
            }

            return await Task.FromResult(_cache);
        }
    }
}
