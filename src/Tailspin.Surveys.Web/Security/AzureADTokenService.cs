// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Tailspin.Surveys.Common;
using Tailspin.Surveys.Common.Configuration;
using Tailspin.Surveys.Web.Configuration;
using Tailspin.Surveys.Web.Logging;

namespace Tailspin.Surveys.Web.Security
{
    /// <summary>
    /// This service helps with the acquisition of access tokens from Azure Active Directory.
    /// </summary>
    public class AzureADTokenService : IAccessTokenService
    {
        private readonly AzureAdOptions _adOptions;
        private readonly ITokenCacheService _tokenCacheService;
        private readonly ILogger _logger;
        // this is used only for using client credentials with assymetric encryption
        private readonly ICredentialService _credentialService;

        public AzureADTokenService(
            IOptions<ConfigurationOptions> options,
            ITokenCacheService tokenCacheService,
            ICredentialService credentialService,
            ILogger<AzureADTokenService> logger)
        {
            _adOptions = options?.Value?.AzureAd;
            _tokenCacheService = tokenCacheService;
            _credentialService = credentialService;
            _logger = logger;
        }

        /// <summary>
        /// This method retrieves the access token for the WebAPI resource that has previously
        /// been retrieved and cached. This method will fail if an access token for the WebAPI 
        /// resource has not been retrieved and cached. You can use the RequestAccessTokenAsync
        /// method to retrieve and cache access tokens.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> for the user to whom the access token belongs.</param>
        /// <returns>A string access token wrapped in a <see cref="Task"/></returns>
        public async Task<string> GetTokenForWebApiAsync(ClaimsPrincipal user)
        {
            return await GetAccessTokenForResourceAsync(_adOptions.WebApiResourceId, user);
        }

        private async Task<string> GetAccessTokenForResourceAsync(string resource, ClaimsPrincipal user)
        {
            var userId = user.GetObjectIdentifierValue();
            var tenantId = user.GetTenantIdValue();
            var userName = user.Identity?.Name;

            try
            {
                _logger.BearerTokenAcquisitionStarted(resource, userName, tenantId);
                var authContext = await CreateAuthenticationContext(user);
                var result = await authContext.AcquireTokenSilentAsync(
                    resource,
                    await _credentialService.GetCredentialsAsync(),
                    new UserIdentifier(userId, UserIdentifierType.UniqueId));

                _logger.BearerTokenAcquisitionSucceeded(resource, userName, tenantId);

                return result.AccessToken;
            }
            catch (AdalException ex)
            {
                _logger.BearerTokenAcquisitionFailed(resource, userName, tenantId, ex);
                throw new AuthenticationException($"AcquireTokenSilentAsync failed for user: {userId}", ex);
            }
        }

        private async Task<AuthenticationContext> CreateAuthenticationContext(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            return new AuthenticationContext(
               Constants.AuthEndpointPrefix + claimsPrincipal.GetTenantIdValue(),
                await _tokenCacheService.GetCacheAsync(claimsPrincipal.GetObjectIdentifierValue(), _adOptions.ClientId));
        }

        /// <summary>
        /// This method acquires an access token using an authorization code and ADAL. The access token is then cached
        /// in a <see cref="TokenCache"/> to be used later (by calls to GetTokenForWebApiAsync).
        /// </summary>
        /// <param name="claimsPrincipal">A <see cref="ClaimsPrincipal"/> for the signed in user</param>
        /// <param name="authorizationCode">a string authorization code obtained when the user signed in</param>
        /// <param name="redirectUri">The Uri of the application requesting the access token</param>
        /// <param name="resource">The resouce identifier of the target resource</param>
        /// <returns></returns>
        public async Task<AuthenticationResult> RequestAccessTokenAsync(
            ClaimsPrincipal claimsPrincipal,
            string authorizationCode,
            string redirectUri,
            string resource)
        {
            if (claimsPrincipal == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            if (string.IsNullOrWhiteSpace(authorizationCode))
            {
                throw new ArgumentNullException(nameof(authorizationCode));
            }

            if (string.IsNullOrWhiteSpace(redirectUri))
            {
                throw new ArgumentNullException(nameof(redirectUri));
            }

            if (string.IsNullOrWhiteSpace(resource))
            {
                throw new ArgumentNullException(nameof(resource));
            }

            try
            {
                var userId = claimsPrincipal.GetObjectIdentifierValue();
                var issuerValue = claimsPrincipal.GetIssuerValue();
                _logger.AuthenticationCodeRedemptionStarted(userId, issuerValue, resource);
                var authenticationContext = await CreateAuthenticationContext(claimsPrincipal);
                var authenticationResult = await authenticationContext.AcquireTokenByAuthorizationCodeAsync(
                    authorizationCode,
                    new Uri(redirectUri),
                    await _credentialService.GetCredentialsAsync(),
                    resource);

                _logger.AuthenticationCodeRedemptionCompleted(userId, issuerValue, resource);
                return authenticationResult;
            }
            catch (Exception ex)
            {
                _logger.AuthenticationCodeRedemptionFailed(ex);
                throw;
            }
        }

        /// <summary>
        /// This method clears the user's <see cref="TokenCache"/>.
        /// </summary>
        /// <param name="claimsPrincipal">The <see cref="ClaimsPrincipal"/> for the user</param>
        /// <returns></returns>
        public async Task ClearCacheAsync(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            await _tokenCacheService.ClearCacheAsync(claimsPrincipal.GetObjectIdentifierValue(), _adOptions.ClientId);
        }
    }
}
