using System;
using System.Security.Claims;
using Microsoft.AspNet.DataProtection;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Tailspin.Surveys.Common;

namespace Tailspin.Surveys.TokenStorage
{
    public class SessionTokenCache : TokenCache
    {
        private const string SessionTokenCacheKey = "Tailspin.Surveys.TokenCache";

        private HttpContext _context;
        private ILogger _logger;
        private ISession _session;

        private IDataProtector _protector;

        /// <summary>
        /// Initializes a new instance of <see cref="Tailspin.Surveys.TokenStorage.SessionTokenCache"/>
        /// </summary>
        /// <param name="contextAccessor">An instance of <see cref="Microsoft.AspNet.Http.IHttpContextAccessor"/> used to get access to the current HTTP context.</param>
        /// <param name="loggerFactory"><see cref="Microsoft.Extensions.Logging.ILoggerFactory"/> used to create type-specific <see cref="Microsoft.Extensions.Logging.ILogger"/> instances.</param>
        /// <param name="dataProtectionProvider">An <see cref="Microsoft.AspNet.DataProtection.IDataProtectionProvider"/> for creating a data protector.</param>
        public SessionTokenCache(IHttpContextAccessor contextAccessor, ILoggerFactory loggerFactory, IDataProtectionProvider dataProtectionProvider)
        {
            Guard.ArgumentNotNull(contextAccessor, nameof(contextAccessor));
            Guard.ArgumentNotNull(loggerFactory, nameof(loggerFactory));
            Guard.ArgumentNotNull(dataProtectionProvider, nameof(dataProtectionProvider));

            _context = contextAccessor.HttpContext;
            _logger = loggerFactory.CreateLogger<SessionTokenCache>();
            _session = contextAccessor.HttpContext.Session;
            _protector = dataProtectionProvider.CreateProtector(typeof(SessionTokenCache).FullName);
            AfterAccess = AfterAccessNotification;
            byte[] sessionData;
            if (_session.TryGetValue(SessionTokenCacheKey, out sessionData))
            {
                this.Deserialize(_protector.Unprotect(sessionData));
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
                    _session.Set(SessionTokenCacheKey, _protector.Protect(this.Serialize()));
                    _logger.TokensWrittenToStore(args.ClientId, args.UniqueId, args.Resource);
                    this.HasStateChanged = false;
                }
                catch (Exception exp)
                {
                    _logger.WriteToCacheFailed(exp);
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
            _session.Remove(SessionTokenCacheKey);
            _logger.TokenCacheCleared(_context.User.GetObjectIdentifierValue(false) ?? "<none>");
        }
    }
}
