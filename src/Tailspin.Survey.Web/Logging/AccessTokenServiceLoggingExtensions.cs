// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Logging;
/// <summary>
/// ILogger extensions for events which occur in the AccessTokenService
/// These events are logged when ADAL is made use of to access tokens
/// </summary>
namespace MultiTenantSurveyApp.Logging
{
    internal static class AccessTokenServiceLoggingExtensions
    {
        public static void BearerTokenAcquisitionStarted(this ILogger logger, string resource, string user, string tenantId)
        {
            logger.LogInformation("Started bearer token acquisition to call webapi: {0} for tenant: {1} user: {2}", resource, tenantId, user);
        }
        public static void BearerTokenAcquisitionSucceeded(this ILogger logger, string resource, string user, string tenantId)
        {
            logger.LogInformation("Succeededed bearer token acquisition to call webapi: {0} for tenant: {1} user: {2}", resource, tenantId, user);
        }
        public static void BearerTokenAcquisitionFailed(this ILogger logger, string resource, string user, string tenantId, Exception exp)
        {
            logger.LogInformation("Succeededed bearer token acquisition to call webapi: {0} for tenant: {1} user: {2}", resource, tenantId, user);
        }
    }
}
