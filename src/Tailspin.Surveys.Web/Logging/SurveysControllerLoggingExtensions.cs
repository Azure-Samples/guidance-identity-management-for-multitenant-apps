// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

namespace Tailspin.Surveys.Web.Logging
{
    /// <summary>
    /// ILogger extensions for events which occur in the SurveysController
    /// </summary>
    internal static class SurveysControllerLoggingExtensions
    {
        public static void GetSurveysForUserOperationStarted(this ILogger logger, string action, string user, string issuer)
        {
            logger.LogInformation("Inside Action: {0} about to call webapi to get list of surveys for User: {1} issuer: {2}", action, user, issuer);
        }

        public static void GetSurveysForUserOperationSucceeded(this ILogger logger, string action, string user, string issuer)
        {
            logger.LogInformation("Inside Action: {0} succeeded called webapi to get list of surveys for User: {1} issuer: {2}", action, user, issuer);
        }

        public static void GetSurveysForUserOperationFailed(this ILogger logger, string action, string user, string issuer, int statusCode)
        {
            logger.LogInformation("Inside Action: {0} failed calling webapi to get list of surveys for User: {1} issuer: {2} StatusCode: {3}", action, user, issuer, statusCode);
        }
    }
}
