using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Tailspin.Surveys.Common;

namespace Tailspin.Surveys.TokenStorage
{
    public class DistributedTokenCacheService : TokenCacheService
    {
        private IHttpContextAccessor _contextAccessor;
        private IDataProtectionProvider _dataProtectionProvider;
        private IDistributedCache _distributedCache;

        /// <summary>
        /// Initializes a new instance of <see cref="Tailspin.Surveys.TokenStorage.DistributedTokenCacheService"/>
        /// </summary>
        /// <param name="contextAccessor">An instance of <see cref="Microsoft.AspNetCore.Http.IHttpContextAccessor"/> used to get access to the current HTTP context.</param>
        /// <param name="loggerFactory"><see cref="Microsoft.Extensions.Logging.ILoggerFactory"/> used to create type-specific <see cref="Microsoft.Extensions.Logging.ILogger"/> instances.</param>
        /// <param name="dataProtectionProvider">An <see cref="Microsoft.AspNetCore.DataProtection.IDataProtectionProvider"/> for creating a data protector.</param>
        public DistributedTokenCacheService(
            IDistributedCache distributedCache,
            IHttpContextAccessor contextAccessor,
            ILoggerFactory loggerFactory,
            IDataProtectionProvider dataProtectionProvider)
            : base(loggerFactory)
        {
            Guard.ArgumentNotNull(distributedCache, nameof(distributedCache));
            Guard.ArgumentNotNull(contextAccessor, nameof(contextAccessor));
            Guard.ArgumentNotNull(dataProtectionProvider, nameof(dataProtectionProvider));
            _distributedCache = distributedCache;
            _contextAccessor = contextAccessor;
            _dataProtectionProvider = dataProtectionProvider;
        }

        /// <summary>
        /// Returns an instance of <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCache"/>.
        /// </summary>
        /// <param name="claimsPrincipal">Current user's <see cref="System.Security.Claims.ClaimsPrincipal"/>.</param>
        /// <returns>An instance of <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCache"/>.</returns>
        public override Task<TokenCache> GetCacheAsync(ClaimsPrincipal claimsPrincipal)
        {
            if (_cache == null)
            {
                _cache = new DistributedTokenCache(claimsPrincipal, _distributedCache, _loggerFactory, _dataProtectionProvider);
            }

            return Task.FromResult(_cache);
        }
    }
}
