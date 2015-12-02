using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiTenantSurveyApp.Configuration.Secrets
{
    /// <summary>
    /// ILogger extensions for events which occur inside the KeyVaultConfigurationProvider
    /// </summary>
    internal static class KeyVaultConfigurationProviderLoggingExtensions
    {
        public static void ConfigurationLoadSuccessful(this ILogger logger, string clientId)
        {
            logger.LogInformation("Configuration loaded successfully for Client: {0}", clientId);
        }
        public static void ConfigurationLoadFailed(this ILogger logger, string clientId, Exception exp)
        {
            logger.LogCritical("Configuration load failed for Client: {0}, Exception: {1}", clientId, exp);
        }
        public static void AuthenticationFailed(this ILogger logger, string clientId, Exception exp)
        {
            logger.LogCritical("Client credential authentication failed for Client: {0}, Exception: {1}", clientId, exp);
        }


    }
}
