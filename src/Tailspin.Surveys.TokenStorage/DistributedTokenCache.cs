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
using System.Threading;
using Microsoft.AspNetCore.Http.Authentication;

namespace Tailspin.Surveys.TokenStorage
{
    public class DistributedTokenCache : TokenCache
    {
        private ClaimsPrincipal _claimsPrincipal;
        private ILogger _logger;
        private IDistributedCache _distributedCache;
        private IDataProtector _protector;
        private string _cacheKey;

        /// <summary>
        /// Initializes a new instance of <see cref="Tailspin.Surveys.TokenStorage.DistributedTokenCache"/>
        /// </summary>
        /// <param name="claimsPrincipal">A <see cref="System.Security.Claims.ClaimsPrincipal"/> for the signed in user</param>
        /// <param name="distributedCache">An implementation of <see cref="Microsoft.Extensions.Caching.Distributed.IDistributedCache"/> in which to store the access tokens.</param>
        /// <param name="loggerFactory"><see cref="Microsoft.Extensions.Logging.ILoggerFactory"/> used to create type-specific <see cref="Microsoft.Extensions.Logging.ILogger"/> instances.</param>
        /// <param name="dataProtectionProvider">An <see cref="Microsoft.AspNetCore.DataProtection.IDataProtectionProvider"/> for creating a data protector.</param>
        public DistributedTokenCache(
            ClaimsPrincipal claimsPrincipal,
            IDistributedCache distributedCache,
            ILoggerFactory loggerFactory,
            IDataProtectionProvider dataProtectionProvider)
            : base()
        {
            Guard.ArgumentNotNull(claimsPrincipal, nameof(claimsPrincipal));
            Guard.ArgumentNotNull(distributedCache, nameof(distributedCache));
            Guard.ArgumentNotNull(loggerFactory, nameof(loggerFactory));
            Guard.ArgumentNotNull(dataProtectionProvider, nameof(dataProtectionProvider));

            _claimsPrincipal = claimsPrincipal;
            _cacheKey = BuildCacheKey(_claimsPrincipal);
            _distributedCache = distributedCache;
            _logger = loggerFactory.CreateLogger<DistributedTokenCache>();
            _protector = dataProtectionProvider.CreateProtector(typeof(DistributedTokenCache).FullName);
            AfterAccess = AfterAccessNotification;
            LoadFromCache();
        }

        /// <summary>
        /// Builds the cache key to use for this item in the distributed cache.
        /// </summary>
        /// <param name="claimsPrincipal">A <see cref="System.Security.Claims.ClaimsPrincipal"/> for the signed in user</param>
        /// <returns>Cache key for this item.</returns>
        private static string BuildCacheKey(ClaimsPrincipal claimsPrincipal)
        {
            Guard.ArgumentNotNull(claimsPrincipal, nameof(claimsPrincipal));

            string clientId = claimsPrincipal.FindFirstValue("aud", true);
            return string.Format(
                "UserId:{0}::ClientId:{1}",
                claimsPrincipal.GetObjectIdentifierValue(),
                clientId);
        }

        /// <summary>
        /// Attempts to load tokens from distributed cache.
        /// </summary>
        private void LoadFromCache()
        {
            byte[] cacheData = _distributedCache.Get(_cacheKey);
            if (cacheData != null)
            {
                this.Deserialize(_protector.Unprotect(cacheData));
                _logger.TokensRetrievedFromStore(_cacheKey);
            }
        }

        /// <summary>
        /// Handles the AfterAccessNotification event, which is triggered right after ADAL accesses the cache.
        /// </summary>
        /// <param name="args">An instance of <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCacheNotificationArgs"/> containing information for this event.</param>
        public void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (this.HasStateChanged)
            {
                try
                {
                    if (this.Count > 0)
                    {
                        _distributedCache.Set(_cacheKey, _protector.Protect(this.Serialize()));
                        _logger.TokensWrittenToStore(args.ClientId, args.UniqueId, args.Resource);
                    }
                    else
                    {
                        // There are no tokens for this user/client, so remove them from the cache.
                        // This was previously handled in an overridden Clear() method, but the built-in Clear() calls this
                        // after the dictionary is cleared.
                        _distributedCache.Remove(_cacheKey);
                        _logger.TokenCacheCleared(_claimsPrincipal.GetObjectIdentifierValue(false) ?? "<none>");
                    }
                    this.HasStateChanged = false;
                }
                catch (Exception exp)
                {
                    _logger.WriteToCacheFailed(exp);
                    throw;
                }
            }
        }
    }
}
