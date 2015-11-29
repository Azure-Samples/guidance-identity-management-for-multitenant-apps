// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace MultiTenantSurveyApp.Logging
{
    /// <summary>
    /// ILogger extensions for events which occur in the SurveysController
    /// </summary>
    internal static class SurveysControllerLoggingExtensions
    {
        public static void GetSurveysForUserOperationStarted(this ILogger logger, string action, string user, string tenantId)
        {
            logger.LogInformation("Inside Action: {0} about to call webapi to get list of surveys for User: {1} Tenant: {2}", action, user, tenantId);
        }

        public static void GetSurveysForUserOperationSucceeded(this ILogger logger, string action, string user, string tenantId)
        {
            logger.LogInformation("Inside Action: {0} succeeded called webapi to get list of surveys for User: {1} Tenant: {2}", action, user, tenantId);
        }

        public static void GetSurveysForUserOperationFailed(this ILogger logger, string action, string user, string tenantId, int statusCode)
        {
            logger.LogInformation("Inside Action: {0} failed calling webapi to get list of surveys for User: {1} TenantId: {2} StatusCode: {3}", action, user, tenantId, statusCode);
        }
    }
}
