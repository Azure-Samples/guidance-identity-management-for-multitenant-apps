// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Logging;
using System.Globalization;

/// <summary>
/// ILogger extensions for events that occur in the AccessTokenService
/// </summary>
namespace Tailspin.Surveys.Web.Logging
{
    internal static class AccessTokenServiceLoggingExtensions
    {
        public static void BearerTokenAcquisitionStarted(this ILogger logger, string resource, string user, string issuer)
        {
            logger.LogInformation("Started bearer token acquisition to call webapi: {0} for issuer: {1} user: {2}", resource, issuer, user);
        }
        public static void BearerTokenAcquisitionSucceeded(this ILogger logger, string resource, string user, string issuer)
        {
            logger.LogInformation("Succeeded bearer token acquisition to call webapi: {0} for issuer: {1} user: {2}", resource, issuer, user);
        }
        public static void BearerTokenAcquisitionFailed(this ILogger logger, string resource, string user, string issuer, Exception exp)
        {
            logger.LogError(string.Format(CultureInfo.InvariantCulture, "Succeeded bearer token acquisition to call webapi: {0} for issuer: {1} user: {2}", resource, issuer, user), exp);
        }
    }
}
