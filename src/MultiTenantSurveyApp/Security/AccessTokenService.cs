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
    // [masimms-roshar pushing back...] This should probably be the "AzureAd" access token service, not a generic one
    // [masimms] What does this class do?
    public class AccessTokenService : IAccessTokenService
    {
        private readonly AzureAdOptions _adOptions;
        private readonly ITokenCacheService _tokenCacheService;
        private readonly ILogger _logger;
        // this is used only for using client credentials with assymetric encryption
        private readonly IAppCredentialService _appCredentialService;

        public AccessTokenService(IOptions<ConfigurationOptions> options,
                                  ITokenCacheService tokenCacheService,
                             IAppCredentialService appCredentialService,
                                  ILogger<AccessTokenService> logger)
        {
            _adOptions = options?.Value?.AzureAd;
            _tokenCacheService = tokenCacheService;
            _appCredentialService = appCredentialService;
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
                //start-symmetric: Use the block of code below for symmetric encryption
                var credential = new ClientCredential(_adOptions.ClientId, _adOptions.ClientSecret);
                var result = await authContext.AcquireTokenSilentAsync(resource, credential,
                    new UserIdentifier(userId, UserIdentifierType.UniqueId));
                //end-symmetric

                //start-asymmetric: Use the block of code below for assymetric encryption, follow config steps in docs before switching to assymetric 
                // var result = await authContext.AcquireTokenSilentAsync(resource,
                await _appCredentialService.GetAsymmetricCredentialsAsync();
                //new UserIdentifier(userId, UserIdentifierType.UniqueId));
                //end-asymmetric

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
                //start-symmetric: Use the block of code below for symmetric encryption
                var authenticationResult = await authenticationContext.AcquireTokenByAuthorizationCodeAsync(
                    authorizationCode,
                    new Uri(redirectUri),
                    new ClientCredential(_adOptions.ClientId, _adOptions.ClientSecret),
                    resource);
                //end-symmetric

                //start-asymmetric: Use the block of code below for assymetric encryption, follow config steps in docs before switching to assymetric 
                //var authenticationResult = await authenticationContext.AcquireTokenByAuthorizationCodeAsync(
                //       authorizationCode,
                //        new Uri(_adOptions.PostLogoutRedirectUri),
                //       await _appCredentialService.GetAsymmetricCredentialsAsync(),
                //        resource);
                //end-asymmetric

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
