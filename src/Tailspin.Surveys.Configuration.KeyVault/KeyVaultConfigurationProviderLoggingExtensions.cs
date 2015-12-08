using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Tailspin.Surveys.Configuration.Secrets
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
            logger.LogCritical(string.Format(CultureInfo.InvariantCulture, "Configuration load failed for Client: {0}", clientId), exp);
        }
        public static void AuthenticationFailed(this ILogger logger, string clientId, Exception exp)
        {
            logger.LogCritical(string.Format(CultureInfo.InvariantCulture, "Client credential authentication failed for Client: {0}", clientId), exp);
        }


    }
}
