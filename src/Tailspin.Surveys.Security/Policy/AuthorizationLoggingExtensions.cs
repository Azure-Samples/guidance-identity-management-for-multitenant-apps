// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Tailspin.Surveys.Security.Policy
{
    /// <summary>
    /// ILogger extensions for logging event which occur during authorization
    /// </summary>
    internal static class AuthorizationLoggingExtensions
    {
        public static void ValidatePermissionsSucceeded(this ILogger logger, string user, string userTenantId, string operationName, IEnumerable<string> permissionValues)
        {
            logger.LogInformation("User: {0} of Tenant: {1} authorized to perform Operation: {2} with Permissions: {3}", user, userTenantId, operationName, permissionValues);
        }
        public static void ValidatePermissionsFailed(this ILogger logger, string user, string userTenantId, string operationName, IEnumerable<string> permissionValues)
        {
            logger.LogError("User: {0} of Tenant: {1} is NOT authorized to perform Operation: {2} with Permissions: {3}", user, userTenantId, operationName, permissionValues);
        }
    }
}
