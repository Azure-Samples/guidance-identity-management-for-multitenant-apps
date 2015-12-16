// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Logging;


namespace Tailspin.Surveys.Web.Logging
{
    /// <summary>
    /// ILogger extensions for events that occur when web api calls are made
    /// </summary>
    internal static class WebApiCallsLoggingExtensions
    {
        public static void RequestSucceeded(this ILogger logger, string method, string uri, TimeSpan elapsedTime, string userId, string issuer)
        {
            logger.LogInformation("Request succeeded to web api Uri: {0} Method: {1} Elapsed Time: {2}ms user: {3} of issuer: {4}", uri, method, elapsedTime.TotalMilliseconds, userId, issuer);
        }

        public static void RequestFailed(this ILogger logger, string method, string uri, TimeSpan elapsedTime, string reasonPhrase, int statusCode, string userId, string issuer)
        {
            logger.LogError("Request failed to web api Uri:{0} Method: {1} Reason: {2} StatusCode {3} Elapsed Time: {4}ms user: {5} of issuer: {6}", uri, method, reasonPhrase, statusCode, elapsedTime.TotalMilliseconds, userId, issuer);
        }
    }
}
