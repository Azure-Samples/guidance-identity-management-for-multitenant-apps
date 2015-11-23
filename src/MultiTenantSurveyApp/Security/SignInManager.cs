// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using MultiTenantSurveyApp.Logging;

namespace MultiTenantSurveyApp.Security
{
    public class SignInManager
    {
        private readonly HttpContext _httpContext;

        private readonly IAccessTokenService _accessTokenService;

        private readonly ILogger _logger;

        public SignInManager(IHttpContextAccessor contextAccessor,
            IAccessTokenService accessTokenService,
            ILogger<SignInManager> logger)
        {
            _httpContext = contextAccessor.HttpContext;
            _accessTokenService = accessTokenService;
            _logger = logger;
        }

        public async Task<IActionResult> SignOutAsync(string redirectUrl = null)
        {
            var userObjectIdentifier = _httpContext.User.GetObjectIdentifierValue();
            var issuer = _httpContext.User.GetTenantIdValue();

            try
            {
                _logger.SignoutStarted(userObjectIdentifier, issuer);

                await _httpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                await _httpContext.Authentication.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
                    new AuthenticationProperties { RedirectUri = redirectUrl });

                await _accessTokenService.ClearCacheAsync(userObjectIdentifier);

                _logger.SignoutCompleted(userObjectIdentifier, issuer);
            }
            catch (Exception exp)
            {
                _logger.SignoutFailed(userObjectIdentifier, issuer, exp);
                return new HttpStatusCodeResult((int)HttpStatusCode.InternalServerError);
            }

            return new EmptyResult();
        }
    }
}
