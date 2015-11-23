// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;

//In a real app we should be logging in the controller with the user Id and tenant Id for each request.
// [masimms-roshar] This is a real app.  Yes, we should.  Here's how:
// - Attach a first point delegating handler that acts as the authentication/authorization interceptor
// - Use that value to attach the tenant identifier as part of the http context
// - In the controllers, extract that tentant and use in logging.
// This is a ship stopper.

namespace MultiTenantSurveyApp.Logging
{
    internal static class WebApiCallsLoggingExtensions
    {
        public static void RequestStarted(this ILogger logger, string method, string uri, string userId, string tenantId)
        {
            logger.LogInformation("Calling web api Uri: {0} Method: {1} user: {2} of tenant: {3}", uri, method, userId, tenantId);
        }

        public static void RequestSucceeded(this ILogger logger, string method, string uri, string userId, string tenantId)
        {
            logger.LogInformation("Request succeeded to web api Uri: {0} Method: {1} user: {2} of tenant: {3}", uri, method, userId, tenantId);
        }

        public static void RequestFailed(this ILogger logger, string method, string uri, string reasonPhrase, string statusCode, string userId, string tenantId)
        {
            logger.LogError("Request failed to web api Uri:{0} Method: {1} Reason: {2} StatusCode {3} user: {4} of tenant: {5}", uri, method, reasonPhrase, statusCode, userId, tenantId);
        }
    }
}
