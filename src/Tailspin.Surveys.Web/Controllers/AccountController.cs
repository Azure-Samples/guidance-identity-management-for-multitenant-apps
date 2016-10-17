// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Tailspin.Surveys.Web.Security;

namespace Tailspin.Surveys.Web.Controllers
{
    /// <summary>
    /// This class provides the MVC controller actions related to account management such as sign up, sign in, and sign out.
    /// This class enables multi-tenant authentication using Azure Active Directory.
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {
        private readonly SignInManager _signInManager;

        public AccountController(SignInManager signInManager)
        {
            _signInManager = signInManager;
        }

        /// <summary>
        /// Starts the AAD/OpenIdConnect authentication flow for a user.
        /// </summary>
        /// <returns>A <see cref="Microsoft.AspNetCore.Mvc.ChallengeResult"/> to authenticate a user with AAD and OpenIdConnect.</returns>
        [AllowAnonymous]
        public IActionResult SignIn()
        {
            return new ChallengeResult(
                OpenIdConnectDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    RedirectUri = Url.Action("SignInCallback", "Account")
                });
        }

        /// <summary>
        /// Signs out a previously authenticated user.
        /// </summary>
        /// <returns>A <see cref="Microsoft.AspNetCore.Mvc.IActionResult"/> containing the result of the sign out operation.</returns>
        public async Task<IActionResult> SignOut()
        {
            var callbackUrl = Url.Action("SignOutCallback", "Account", values: null, protocol: Request.Scheme);
            return await _signInManager.SignOutAsync(callbackUrl);
        }

        /// <summary>
        /// Starts the tenant registration flow.
        /// </summary>
        /// <returns>A <see cref="Microsoft.AspNetCore.Mvc.ChallengeResult"/> to authenticate a user with AAD and OpenIdConnect.</returns>
        [AllowAnonymous]
        public IActionResult SignUp()
        {
            var state = new Dictionary<string, string> { { "signup", "true" }};
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
        /// <returns>A <see cref="Microsoft.AspNetCore.Mvc.IActionResult"/> containing the result of the sign out operation.</returns>
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
        /// <returns>A <see cref="Microsoft.AspNetCore.Mvc.RedirectToActionResult"/> representing where to redirect the user after sign out has completed.</returns>
        [AllowAnonymous]
        public IActionResult SignOutCallback()
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        /// <summary>
        /// Callback method used when a user has been successfully authenticated.  This can be used for any sign in post-processing.
        /// </summary>
        /// <remarks>Any modifications to the user's <see cref="System.Security.Claims.ClaimsPrincipal"/> within this callback will not be persisted across requests.</remarks>
        /// <returns>A <see cref="Microsoft.AspNetCore.Mvc.RedirectToActionResult"/> representing where to redirect the user after authentication has completed.</returns>
        [HttpGet]
        public IActionResult SignInCallback(string returnUrl = null)
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
