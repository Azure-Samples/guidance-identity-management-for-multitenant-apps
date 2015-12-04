// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Tailspin.Surveys.Security;

namespace System.Security.Claims
{
    public static class ClaimsPrincipalExtensions
    {
        public static string FindFirstValue(this ClaimsPrincipal principal, string claimType, bool throwIfNotFound = false)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            var value = principal.FindFirst(claimType)?.Value;
            if (throwIfNotFound && string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.InvariantCulture, "The supplied principal does not contain a claim of type {0}", claimType));
            }

            return value;
        }

        public static string GetIssuerValue(this ClaimsPrincipal principal)
        {
            // The "iss" claim is REQUIRED by OIDC, so we're going to throw an exception if we don't have the claim OR the value.
            // Per http://openid.net/specs/openid-connect-core-1_0.html#IDToken
            return principal.FindFirstValue("iss", true);
        }

        public static string GetTenantIdValue(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(SurveyClaimTypes.TenantId, true);
        }

        public static string GetObjectIdentifierValue(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(SurveyClaimTypes.ObjectId, true);
        }

        public static string GetDisplayNameValue(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue("name", true);
        }

        public static string GetEmailValue(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.Email, true);
        }

        public static int GetUserKey(this ClaimsPrincipal principal)
        {
            return (int)Convert.ChangeType(principal.FindFirstValue(SurveyClaimTypes.SurveyUserIdClaimType, true), typeof(int));
        }

        public static string GetUserName(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            return principal.FindFirstValue(ClaimsIdentity.DefaultNameClaimType);
        }

        public static bool IsSignedInToApplication(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }
            return principal.Identity != null && principal.Identity.IsAuthenticated;
            //return principal?.Identities != null &&
            //    principal.Identities.Any(i => i.AuthenticationType == CookieAuthenticationDefaults..ApplicationCookieAuthenticationType);
        }
    }
}
