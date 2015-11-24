// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Mvc;
using MultiTenantSurveyApp.DAL.DataModels;
using MultiTenantSurveyApp.Security;

namespace MultiTenantSurveyApp.Controllers
{
    /// <summary>
    /// This class provides the MVC controller actions related to account management such as sign up, sign in, and sign out.
    /// This class enables multi-tenant authentication using Azure Active Directory.
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {
        private readonly TenantManager _tenantManager;
        private readonly SignInManager _signInManager;

        public AccountController(TenantManager tenantManager, SignInManager signInManager)
        {
            _tenantManager = tenantManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Starts the AAD/OpenIdConnect authentication flow for a user.
        /// </summary>
        /// <returns>A <see cref="Microsoft.AspNet.Mvc.ChallengeResult"/> to authenticate a user with AAD and OpenIdConnect.</returns>
        [AllowAnonymous]
        public IActionResult SignIn()
        {
            return new ChallengeResult(
                OpenIdConnectDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("SignInCallback", "Account")
                });
        }

        /// <summary>
        /// Signs out a previously authenticated user.
        /// </summary>
        /// <returns>A <see cref="Microsoft.AspNet.Mvc.IActionResult"/> containing the result of the sign out operation.</returns>
        public async Task<IActionResult> SignOut()
        {
            var callbackUrl = Url.Action("SignOutCallback", "Account", values: null, protocol: Request.Scheme);
            return await _signInManager.SignOutAsync(callbackUrl);
        }

        /// <summary>
        /// Generates a cross site request forgery token used to verify the sign up request.
        /// </summary>
        /// <returns>A temporary <see cref="MultiTenantSurveyApp.DAL.DataModels.Tenant"/> containing the CSRF token.</returns>
        private static Tenant GenerateCsrfTenant()
        {
            // We need to generate a state that we can pass along so we know the request came from us
            return new Tenant
            {
                IssuerValue = Guid.NewGuid().ToString(),
                Created = DateTimeOffset.UtcNow
            };
        }

        /// <summary>
        /// Starts the tenant registration flow.
        /// </summary>
        /// <returns>A <see cref="Microsoft.AspNet.Mvc.ChallengeResult"/> to authenticate a user with AAD and OpenIdConnect.</returns>
        [AllowAnonymous]
        public async Task<IActionResult> SignUp()
        {
            var tenant = GenerateCsrfTenant();
            try
            {
                await _tenantManager.CreateAsync(tenant);
            }
            catch
            {
                // TODO - Handle a failure to write to the database
                // [masimms] Yes, this a great todo.  At the very least, log the exception.
                // TODO: Ask Product Group
                throw new InvalidOperationException("Unable to write temporary tenant to database");
            }

            // Workaround for https://github.com/aspnet/Security/issues/546
            HttpContext.Items.Add("signup", "true");

            var state = new Dictionary<string, string> { { "signup", "true" }, { "csrf_token", tenant.IssuerValue } };
            return new ChallengeResult(
                OpenIdConnectDefaults.AuthenticationScheme,
                new AuthenticationProperties(state)
                {
                    RedirectUri = Url.Action(nameof(SignUpCallback), "Account")
                });
        }

        /// <summary>
        /// Callback method used by the tenant sign up flow.  This can be used for any sign up post-processing work.
        /// </summary>
        /// <param name="returnUrl">Unused in the current implementation.</param>
        /// <returns>A <see cref="Microsoft.AspNet.Mvc.IActionResult"/> containing the result of the sign out operation.</returns>
        [HttpGet]
        public async Task<IActionResult> SignUpCallback(string returnUrl = null)
        {
            // Because of app roles, we need to sign out the user and redirect them to the instructions page.
            var redirectUrl = Url.Action("SignUpSuccess", "Account", values: null, protocol: Request.Scheme);
            return await _signInManager.SignOutAsync(redirectUrl);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult SignUpSuccess()
        {
            return View();
        }

        /// <summary>
        /// Callback method used when a previously authenticated user is signed up.  This can be used for any sign out post-processing.
        /// </summary>
        /// <returns>A <see cref="Microsoft.AspNet.Mvc.RedirectToActionResult"/> representing where to redirect the user after sign out has completed.</returns>
        [AllowAnonymous]
        public IActionResult SignOutCallback()
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        /// <summary>
        /// Callback method used when a user has been successfully authenticated.  This can be used for any sign in post-processing.
        /// </summary>
        /// <remarks>Any modifications to the user's <see cref="System.Security.Claims.ClaimsPrincipal"/> within this callback will not be persisted across requests.</remarks>
        /// <returns>A <see cref="Microsoft.AspNet.Mvc.RedirectToActionResult"/> representing where to redirect the user after authentication has completed.</returns>
        [HttpGet]
        public IActionResult SignInCallback(string returnUrl = null)
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
