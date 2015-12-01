// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MultiTenantSurveyApp.Common;
using MultiTenantSurveyApp.Common.Configuration;
using MultiTenantSurveyApp.Configuration;
using MultiTenantSurveyApp.Logging;

namespace MultiTenantSurveyApp.Security
{
    /// <summary>
    /// 
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

        public async Task<AuthenticationResult> CacheAccessTokenAsync(
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

        public async Task ClearCacheAsync(string userObjectId)
        {
            await _tokenCacheService.ClearCacheAsync(userObjectId, _adOptions.ClientId);
        }
    }
}
