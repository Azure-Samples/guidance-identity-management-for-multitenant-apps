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

        public async Task<IActionResult> SignOut()
        {
            var callbackUrl = Url.Action("SignOutCallback", "Account", values: null, protocol: Request.Scheme);
            return await _signInManager.SignOutAsync(callbackUrl);
        }

        private static Tenant GenerateCsrfTenant()
        {
            // We need to generate a state that we can pass along so we know the request came from us
            return new Tenant
            {
                IssuerValue = Guid.NewGuid().ToString(),
                Created = DateTimeOffset.UtcNow
            };
        }

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

        [AllowAnonymous]
        public IActionResult SignOutCallback()
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        public IActionResult SignInCallback(string returnUrl = null)
        {
            return RedirectToLocal(returnUrl);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
