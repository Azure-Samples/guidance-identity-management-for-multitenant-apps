// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.Logging;

namespace Tailspin.Surveys.TokenStorage
{
    /// <summary>
    /// ILogger extensions for events which occur in the RedisTokenCache
    /// </summary>
    internal static class RedisTokenCacheLoggingExtensions
    {
        public static void ReadFromCacheFailed(this ILogger logger, Exception exp)
        {
            logger.LogError("Reading from redis cache failed", exp);
        }
        public static void WriteToRedisCacheFailed(this ILogger logger, Exception exp)
        {
            logger.LogError("Write to redis cache failed", exp);
        }
        public static void TokenCacheCleared(this ILogger logger, string key)
        {
            logger.LogInformation("Cleared token cache for key {0}", key);
        }
        public static void TokensRetrievedFromStore(this ILogger logger, string key)
        {
            logger.LogVerbose("Retrieved all tokens from store for key {0}", key);
        }
        public static void TokensWrittenToStore(this ILogger logger, string clientId, string userId, string resource)
        {
            logger.LogVerbose("Token states changed for Client :{0} User: {1}  Resource: {2} writing all tokens back to store", clientId, userId, resource);
        }
    }
}
