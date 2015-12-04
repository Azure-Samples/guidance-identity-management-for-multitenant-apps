// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Logging;

namespace Tailspin.Surveys.Web.Logging
{
    /// <summary>
    /// ILogger extensions for events which occur in the AccountController
    /// </summary>
    internal static class AccountControllerLoggingExtensions
    {
        public static void SignoutStarted(this ILogger logger, string user, string issuer)
        {
            logger.LogInformation("About to sign out user: {0} of tenant: {1}", user, issuer);
        }
        public static void SignoutCompleted(this ILogger logger, string user, string issuer)
        {
            logger.LogInformation("Signed out user: {0} of tenant: {1}", user, issuer);
        }
        public static void SignoutFailed(this ILogger logger, string user, string issuer, Exception exp)
        {
            logger.LogError("signout failed for user: {0}  of tenant: {1} with error: {2}", user, issuer, exp);
        }
    }
}
