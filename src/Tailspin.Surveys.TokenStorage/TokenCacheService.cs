// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Tailspin.Surveys.Common;

namespace Tailspin.Surveys.TokenStorage
{
    /// <summary>
    /// Returns and manages the instance of token cache to be used when making use of ADAL. 
    /// This returns the default token cache in the case of DNX core and Redis token cache when using DNX451 since Redis client is supported only in DNX451
    /// Lifetime should be scoped- we need a new instance for every request if we are using the redis cache
    public abstract class TokenCacheService : ITokenCacheService
    {
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly ILoggerFactory _loggerFactory;
        protected readonly ILogger _logger;
        protected TokenCache _cache = null;

        /// <summary>
        /// Initializes a new instance of <see cref="Tailspin.Surveys.TokenStorage.TokenCacheService"/>
        /// </summary>
        /// <param name="contextAccessor"><see cref="Microsoft.AspNet.Http.IHttpContextAccessor"/> used to access the current <see cref="Microsoft.AspNet.Http.HttpContext"/></param>
        /// <param name="loggerFactory"><see cref="Microsoft.Extensions.Logging.ILoggerFactory"/> used to create type-specific <see cref="Microsoft.Extensions.Logging.ILogger"/> instances.</param>
        protected TokenCacheService(IHttpContextAccessor contextAccessor, ILoggerFactory loggerFactory)
        {
            Guard.ArgumentNotNull(contextAccessor, nameof(contextAccessor));
            Guard.ArgumentNotNull(loggerFactory, nameof(loggerFactory));

            _httpContextAccessor = contextAccessor;
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger(this.GetType().FullName);
        }

        /// <summary>
        /// Returns an instance of <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCache"/>.
        /// </summary>
        /// <param name="userObjectId">Azure Active Directory user's ObjectIdentifier.</param>
        /// <param name="clientId">Azure Active Directory ApplicationId.</param>
        /// <returns>An instance of <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCache"/>.</returns>
        public abstract Task<TokenCache> GetCacheAsync(string userObjectId, string clientId);

        /// <summary>
        /// Clears the token cache.
        /// </summary>
        /// <param name="userObjectId">Azure Active Directory user's ObjectIdentifier.</param>
        /// <param name="clientId">Azure Active Directory ApplicationId.</param>
        public virtual async Task ClearCacheAsync(string userObjectId, string clientId)
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
