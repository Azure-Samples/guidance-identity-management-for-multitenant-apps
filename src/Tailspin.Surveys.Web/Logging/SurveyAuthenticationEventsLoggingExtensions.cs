// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace Tailspin.Surveys.Web.Logging
{
    /// <summary>
    /// ILogger extensions for events which occur when OpenID Authentication notifications occur.
    /// </summary>
    internal static class SurveyAuthenticationEventsLoggingExtensions
    {
        public static void RedirectToIdentityProvider(this ILogger logger)
        {
            logger.LogInformation("Redirecting to Identity Provider");
        }

        public static void AuthenticationValidated(this ILogger logger, string userId, string issuer)
        {
            logger.LogInformation("Auth token validated for User:{0}, Issuer: {1}", userId, issuer);
        }

        public static void UnregisteredUserSignInAttempted(this ILogger logger, string userId, string issuer)
        {
            logger.LogWarning("User {0} of Issuer {1} is not from a registered tenant", userId, issuer);
        }

        public static void AuthenticationCodeRedemptionStarted(this ILogger logger, string userId, string issuer, string resource)
        {
            logger.LogInformation("About to redeem Auth Code for Resource: {0} using authorization code of User: {1} of Issuer: {2}", resource, userId, issuer);
        }

        public static void AuthenticationCodeRedemptionCompleted(this ILogger logger, string userId, string issuer, string resource)
        {
            logger.LogInformation("Completed redeeming Auth Code for Resource: {0} for User: {1} of Issuer: {2}", resource, userId, issuer);
        }

        public static void AuthenticationCodeRedemptionFailed(this ILogger logger, Exception exp)
        {
            logger.LogError("Failed redeeming auth code", exp);
        }

        public static void AuthenticationFailed(this ILogger logger, Exception exp)
        {
            logger.LogError("Authentication failed", exp);
        }

        public static void SignUpTenantFailed(this ILogger logger, string userId, string issuer, Exception exp)
        {
            logger.LogError(string.Format(CultureInfo.InvariantCulture, "Tenant SignUp request failed for User: {0} of Issuer: {1}", userId, issuer), exp);
        }
    }
}
