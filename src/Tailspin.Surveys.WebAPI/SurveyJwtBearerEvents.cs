// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tailspin.Surveys.Data.DataModels;
using Tailspin.Surveys.WebAPI.Logging;

namespace Tailspin.Surveys.WebApi
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

        // This override is left in the codebase to allow the user to set a breakpoint
        // that gets hit at the beginning of the Jwt Bearer token validation process.
        public override Task ReceivingToken(ReceivingTokenContext context)
        {
            return base.ReceivingToken(context);
        }

        public override Task ReceivedToken(ReceivedTokenContext context)
        {
            _logger.TokenReceived();
            return base.ReceivedToken(context);
        }

        /// <summary>
        /// This method contains the logic that validates the user's tenant and normalizes claims.
        /// </summary>
        /// <param name="context">The validated token context</param>
        /// <returns>A task</returns>
        public override async Task ValidatedToken(ValidatedTokenContext context)
        {
            var principal = context.AuthenticationTicket.Principal;
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
            identity.AddClaim(new Claim("survey_userid", registeredUser.Id.ToString()));

            // Adding new Claim for Email
            var email = principal.FindFirst(ClaimTypes.Upn)?.Value;
            if (!string.IsNullOrWhiteSpace(email))
            {
                identity.AddClaim(new Claim(ClaimTypes.Email, email));
            }
            _logger.TokenValidationSucceeded(principal.GetObjectIdentifierValue(), issuerValue, tenant.Id);
        }
    }
}
