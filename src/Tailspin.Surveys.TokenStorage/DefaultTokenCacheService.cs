using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Tailspin.Surveys.TokenStorage
{
    /// <summary>
    /// Returns the default token cache which is part of ADAL
    /// </summary>
    public class DefaultTokenCacheService : TokenCacheService
    {
        /// <summary>
        /// Creates a new instance of <see cref="Tailspin.Surveys.TokenStorage.DefaultTokenCacheService"/>
        /// </summary>
        /// <param name="contextAccessor"><see cref="Microsoft.AspNet.Http.IHttpContextAccessor"/> used to access the current <see cref="Microsoft.AspNet.Http.HttpContext"/></param>
        /// <param name="loggerFactory"><see cref="Microsoft.Extensions.Logging.ILoggerFactory"/> used to create type-specific <see cref="Microsoft.Extensions.Logging.ILogger"/> instances.</param>
        public DefaultTokenCacheService(IHttpContextAccessor contextAccessor, ILoggerFactory loggerFactory)
            : base(contextAccessor, loggerFactory)
        {
            _cache = TokenCache.DefaultShared;
        }

        /// <summary>
        /// Returns an instance of <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCache"/>.
        /// </summary>
        /// <param name="userObjectId">Azure Active Directory user's ObjectIdentifier.</param>
        /// <param name="clientId">Azure Active Directory ApplicationId.</param>
        /// <returns>An instance of <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCache"/>.</returns>
        public override Task<TokenCache> GetCacheAsync(string userObjectId, string clientId)
        {
            return Task.FromResult(_cache);
        }
    }
}
