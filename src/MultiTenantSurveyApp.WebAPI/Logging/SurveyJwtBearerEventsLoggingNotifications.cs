// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Logging;

namespace MultiTenantSurveyApp.WebAPI.Logging
{
    internal static class SurveyJwtBearerEventsLoggingNotifications
    {
        public static void AuthenticationFailed(this ILogger logger, Exception e)
        {
            logger.LogError("Authentication failed Exception: {0}", e);
        }
        public static void TokenReceived(this ILogger logger)
        {
            logger.LogInformation("Received a bearer token");
        }
        public static void TokenValidationStarted(this ILogger logger, string userId, string issuer)
        {
            logger.LogInformation("Token Validation Started for User: {0} Issuer: {1} strings {2}", userId, issuer);
        }
        public static void TokenValidationFailed(this ILogger logger, string userId, string issuer)
        {
            logger.LogWarning("Tenant is not registered User: {0} Issuer: {1}", userId, issuer);
        }
        public static void TokenValidationSucceeded(this ILogger logger, string userId, string issuer, int tenantId)
        {
            logger.LogInformation("Token validation succeeded: User: {0} Issuer: {1} TenantId: {2}", userId, issuer, tenantId);
        }
    }
}
