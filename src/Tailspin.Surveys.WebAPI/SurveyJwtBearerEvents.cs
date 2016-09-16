// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tailspin.Surveys.Data.DataModels;
using Tailspin.Surveys.Security;
using Tailspin.Surveys.WebAPI.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Tailspin.Surveys.WebAPI
{
    /// <summary>
    /// This class extends JwtBearerEvents and provides the logic that is executed at various stages when the Jwt Bearer token is validated.
    /// </summary>
    public class SurveysJwtBearerEvents : JwtBearerEvents
    {
        private readonly ILogger _logger;

        public SurveysJwtBearerEvents(ILogger logger)
        {
            _logger = logger;
        }

        public override Task AuthenticationFailed(AuthenticationFailedContext context)
        {
            _logger.AuthenticationFailed(context.Exception);
            return base.AuthenticationFailed(context);
        }

        // Replaced: https://github.com/aspnet/Security/commit/3f596108aac3d8fc7fb40d39e19a7f897a90c198
        public override Task MessageReceived(MessageReceivedContext context)
        {
            _logger.TokenReceived();
            return base.MessageReceived(context);
        }

        /// <summary>
        /// This method contains the logic that validates the user's tenant and normalizes claims.
        /// </summary>
        /// <param name="context">The validated token context</param>
        /// <returns>A task</returns>
        public override async Task TokenValidated(TokenValidatedContext context)
        {
            var principal = context.Ticket.Principal;
            var tenantManager = context.HttpContext.RequestServices.GetService<TenantManager>();
            var userManager = context.HttpContext.RequestServices.GetService<UserManager>();
            var issuerValue = principal.GetIssuerValue();
            var tenant = await tenantManager.FindByIssuerValueAsync(issuerValue);

            // the caller comes from an admin-consented, recorded issuer
            if (tenant == null)
            {
                _logger.TokenValidationFailed(principal.GetObjectIdentifierValue(), issuerValue);
                // the caller was not from a trusted issuer - throw to block the authentication flow
                throw new SecurityTokenValidationException();
            }

            var identity = principal.Identities.First();

            // Adding new Claim for survey_userid
            var registeredUser = await userManager.FindByObjectIdentifier(principal.GetObjectIdentifierValue());
            identity.AddClaim(new Claim(SurveyClaimTypes.SurveyUserIdClaimType, registeredUser.Id.ToString()));
            identity.AddClaim(new Claim(SurveyClaimTypes.SurveyTenantIdClaimType, registeredUser.TenantId.ToString()));

            // Adding new Claim for Email
            var email = principal.FindFirst(ClaimTypes.Upn)?.Value;
            if (!string.IsNullOrWhiteSpace(email))
            {
                identity.AddClaim(new Claim(ClaimTypes.Email, email));
            }
            _logger.TokenValidationSucceeded(principal.GetObjectIdentifierValue(), issuerValue);
        }
    }
}
