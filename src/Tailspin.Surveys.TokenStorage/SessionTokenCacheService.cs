using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.DataProtection;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Tailspin.Surveys.Common;

namespace Tailspin.Surveys.TokenStorage
{
    public class SessionTokenCacheService : TokenCacheService
    {
        private IHttpContextAccessor _contextAccessor;

        private IDataProtectionProvider _dataProtectionProvider;

        /// <summary>
        /// Initializes a new instance of <see cref="Tailspin.Surveys.TokenStorage.SessionTokenCacheService"/>
        /// </summary>
        /// <param name="contextAccessor">An instance of <see cref="Microsoft.AspNet.Http.IHttpContextAccessor"/> used to get access to the current HTTP context.</param>
        /// <param name="loggerFactory"><see cref="Microsoft.Extensions.Logging.ILoggerFactory"/> used to create type-specific <see cref="Microsoft.Extensions.Logging.ILogger"/> instances.</param>
        /// <param name="dataProtectionProvider">An <see cref="Microsoft.AspNet.DataProtection.IDataProtectionProvider"/> for creating a data protector.</param>
        public SessionTokenCacheService(IHttpContextAccessor contextAccessor, ILoggerFactory loggerFactory, IDataProtectionProvider dataProtectionProvider)
            : base(loggerFactory)
        {
            Guard.ArgumentNotNull(contextAccessor, nameof(contextAccessor));
            Guard.ArgumentNotNull(dataProtectionProvider, nameof(dataProtectionProvider));
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
                _cache = new SessionTokenCache(_contextAccessor, _loggerFactory, _dataProtectionProvider);
            }

            return Task.FromResult(_cache);
        }
    }
}
