// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiTenantSurveyApp.Common.Configuration;
using MultiTenantSurveyApp.DAL.DataModels;
using MultiTenantSurveyApp.Logging;

namespace MultiTenantSurveyApp.Security
{
    /// <summary>
    /// [masimms] What does this class do?
    /// </summary>
    public class SurveyAuthenticationEvents : OpenIdConnectEvents
    {
        private readonly AzureAdOptions _adOptions;
        private readonly ILogger _logger;

        public SurveyAuthenticationEvents(AzureAdOptions adOptions, ILogger logger)
        {
            _adOptions = adOptions;
            _logger = logger;
        }

        // [masimms-andrew] Add a short blurb on how these methods are for making it easier to 
        // debug the flow.  That is really great insight.

        // ReSharper disable RedundantOverridenMember
        public override Task MessageReceived(MessageReceivedContext context)
        {
            return base.MessageReceived(context);
        }

        public override Task RedirectToEndSessionEndpoint(RedirectContext context)
        {
            return base.RedirectToEndSessionEndpoint(context);
        }
        // ReSharper restore RedundantOverridenMember

        // [masimms] What is "special" here (why is this not default behavior)
        public override Task RedirectToAuthenticationEndpoint(RedirectContext context)
        {
            if (context.IsSigningUp())
            {
                context.ProtocolMessage.Prompt = "admin_consent";
            }

            _logger.RedirectToIdentityProvider();
            return Task.FromResult(0);
        }

        // [masimms] What does this method do?
        private static void NormalizeClaims(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            var identity = principal.Identities.First();
            if (!identity.IsAuthenticated)
            {
                throw new InvalidOperationException("The supplied principal is not authenticated.");
            }

            var email = principal.FindFirst(ClaimTypes.Upn)?.Value;
            if (!string.IsNullOrWhiteSpace(email))
            {
                identity.AddClaim(new Claim(ClaimTypes.Email, email));
            }

            // We need to normalize the name claim for the Identity model
            var name = principal.GetDisplayNameValue();
            if (!string.IsNullOrWhiteSpace(name))
            {
                // It looks like AAD does something strange here, but it's actually the JwtSecurityTokenHandler making assumptions
                // about the claims from AAD.  It takes the unique_name claim from AAD and maps it to a ClaimTypes.Name claim, which
                // is the default type for a name claim for this identity.  If we don't remove the old one, there will be two name claims,
                // so let's get rid of the first one.
                // EDIT - We shouldn't do this yet, as it might muck with the identity stuff.
                //var previousNameClaim = principal.FindFirst(ClaimTypes.Name);
                //if (previousNameClaim != null)
                //{
                //    identity.RemoveClaim(previousNameClaim);
                //}

                identity.AddClaim(new Claim(identity.NameClaimType, name));
            }
        }

        // [masimms] What does this method do?
        private async Task<Tenant> ValidateSignUpRequestAsync(AuthenticationTicket authenticationTicket, TenantManager tenantManager)
        {
            if (authenticationTicket == null)
            {
                throw new ArgumentNullException(nameof(authenticationTicket));
            }

            // Get our CSRF token to verify access
            // [masimms] Link to details on CSRF 
            string csrfToken;
            if ((!authenticationTicket.Properties.Items.TryGetValue("csrf_token", out csrfToken)) || (string.IsNullOrWhiteSpace(csrfToken)))
            {
                // TODO Fix error handling
                throw new InvalidOperationException("Missing csrf_token");
            }

            // See if we made this request.  If tenant is null, we did not initiate the request flow - throw an error
            var tenant = await tenantManager.FindByIssuerValueAsync(csrfToken);
            if (tenant == null)
            {
                // TODO - Fix Error handling
                throw new InvalidOperationException("Invalid CSRF token.");
            }

            return tenant;
        }

        private async Task<Tenant> SignUpTenantAsync(BaseControlContext context, TenantManager tenantManager)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (tenantManager == null)
            {
                throw new ArgumentNullException(nameof(tenantManager));
            }

            var tenant = await ValidateSignUpRequestAsync(context.AuthenticationTicket, tenantManager);
            try
            {
                var principal = context.AuthenticationTicket.Principal;
                tenant.IssuerValue = principal.GetIssuerValue();
                await tenantManager.UpdateTenantAsync(tenant);
            }
            catch
            {
                // Something happened when we were setting up our tenant, so we need to clean up the database.
                // [masimms] What happens if there is an exception in this flow?  Is there a higher order handler?
                await tenantManager.DeleteTenantAsync(tenant);

                throw;
            }

            return tenant;
        }

        private async Task CreateOrUpdateUserAsync(AuthenticationTicket authenticationTicket, UserManager userManager, Tenant tenant)
        {
            if (authenticationTicket == null)
            {
                throw new ArgumentNullException(nameof(authenticationTicket));
            }

            if (userManager == null)
            {
                throw new ArgumentNullException(nameof(userManager));
            }

            if (tenant == null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            var principal = authenticationTicket.Principal;
            string objectIdentifier = principal.GetObjectIdentifierValue();
            string displayName = principal.GetDisplayNameValue();
            string email = principal.GetEmailValue();

            var user = await userManager.FindByObjectIdentifier(objectIdentifier);
            if (user == null)
            {
                // The user isn't in our database, so add them.
                user = new User
                {
                    Created = DateTimeOffset.UtcNow,
                    ObjectId = objectIdentifier,
                    TenantId = tenant.Id,
                    DisplayName = displayName,
                    Email = email
                };

                await userManager.CreateAsync(user);
            }
            else
            {
                // Since we aren't the system of record, we need to attempt to keep our display values in sync with the user store.
                // We'll do a simple form of it here.
                bool shouldSaveUser = false;
                if (!user.DisplayName.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                {
                    user.DisplayName = displayName;
                    shouldSaveUser = true;
                }

                // Do a case insensitive comparison for email matching
                if (!user.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                {
                    user.Email = email;
                    shouldSaveUser = true;
                }

                if (shouldSaveUser)
                {
                    await userManager.UpdateAsync(user);
                }
            }

            // Add in the survey id claim.
            principal.Identities.First().AddClaim(new Claim(SurveyClaimTypes.SurveyUserIdClaimType, user.Id.ToString()));
        }

        // [masimms] What does this method do?
        public override async Task AuthenticationValidated(AuthenticationValidatedContext context)
        {
            var principal = context.AuthenticationTicket.Principal;
            var accessTokenService = context.HttpContext.RequestServices.GetService<IAccessTokenService>();
            try
            {
                var userId = principal.GetObjectIdentifierValue();
                var tenantManager = context.HttpContext.RequestServices.GetService<TenantManager>();
                var userManager = context.HttpContext.RequestServices.GetService<UserManager>();
                var issuerValue = principal.GetIssuerValue();
                _logger.AuthenticationValidated(userId, issuerValue);

                // Normalize the claims first.
                NormalizeClaims(principal);
                var tenant = await tenantManager.FindByIssuerValueAsync(issuerValue);

                // Validate the process flow
                // [masimms] Which process?
                if (context.IsSigningUp())
                {
                    // Originally, we were checking to see if the tenant was non-null, however, this would not allow
                    // permission changes to the application in AAD since a re-consent may be required.  Now we just don't
                    // try to recreate the tenant.
                    if (tenant == null)
                    {
                        tenant = await SignUpTenantAsync(context, tenantManager);
                    }

                    // In this case, we need to go ahead and set up the user signing us up.
                    await CreateOrUpdateUserAsync(context.AuthenticationTicket, userManager, tenant);
                }
                else
                {
                    if (tenant == null)
                    {
                        _logger.UnregisteredUserSignInAttempted(userId, issuerValue);
                        throw new SecurityTokenValidationException($"Tenant {issuerValue} is not registered");
                    }

                    await CreateOrUpdateUserAsync(context.AuthenticationTicket, userManager, tenant);
                    // [mattjoh] now what is done with the user?

                    // We are good, so cache our token for Web Api now.
                    await accessTokenService.CacheAccessTokenAsync(
                        principal,
                        context.ProtocolMessage.Code,
                        context.AuthenticationTicket.Properties.Items[OpenIdConnectDefaults.RedirectUriForCodePropertiesKey],
                        _adOptions.WebApiResourceId);
                }

            }
            catch
            {
                // If an exception is thrown within this event, the user is never set on the OWIN middleware,
                // so there is no need to sign out.  However, the access token could have been put into the
                // cache so we need to clean it up.
                await accessTokenService.ClearCacheAsync(principal.GetObjectIdentifierValue());
                throw;
            }
        }

        public override Task AuthenticationFailed(AuthenticationFailedContext context)
        {
            _logger.AuthenticationFailed(context.Exception);
            return Task.FromResult(0);
        }
    }
}
