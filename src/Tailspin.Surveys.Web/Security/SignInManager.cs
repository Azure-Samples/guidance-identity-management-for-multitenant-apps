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
using Tailspin.Surveys.Web.Logging;

namespace Tailspin.Surveys.Web.Security
{
    public class SignInManager
    {
        private readonly HttpContext _httpContext;

        private readonly IAccessTokenService _accessTokenService;

        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="Tailspin.Surveys.Web.Security.SignInManager"/>;
        /// </summary>
        /// <param name="contextAccessor">An instance of <see cref="Microsoft.AspNet.Http.IHttpContextAccessor"/> used to get access to the current HTTP context.</param>
        /// <param name="accessTokenService">An instance of <see cref="Tailspin.Surveys.Web.Security.IAccessTokenService"/></param>
        /// <param name="logger">An <see cref="Microsoft.Extensions.Logging.ILogger"/> implementation used for diagnostic information.</param>
        public SignInManager(IHttpContextAccessor contextAccessor,
            IAccessTokenService accessTokenService,
            ILogger<SignInManager> logger)
        {
            _httpContext = contextAccessor.HttpContext;
            _accessTokenService = accessTokenService;
            _logger = logger;
        }

        /// <summary>
        /// Signs the currently signed in principal out of all authentication schemes and clears any access tokens from the token cache.
        /// </summary>
        /// <param name="redirectUrl">A Url to which the user should be redirected when sign out of AAD completes.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task{Microsoft.AspNet.Mvc.IActionResult}"/> implementation.</returns>
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
