// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Tailspin.Surveys.Configuration.Secrets
{
    public static class KeyVaultConfigurationExtensions
    {

        /// <summary>
        /// Load the config provider which reads shared secret configuration from key vault making use of cert loaded a specified cert store location
        /// </summary>
        /// <param name="configurationBuilder"></param>
        /// <param name="appClientId"></param>
        /// <param name="vaultName"></param>
        /// <param name="storeName"></param>
        /// <param name="storeLocation"></param>
        /// <param name="certificateThumbprint"></param>
        /// <param name="validateCertificate"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddKeyVaultSecrets(this IConfigurationBuilder configurationBuilder, string appClientId, string vaultName, StoreName storeName, StoreLocation storeLocation, string certificateThumbprint, bool validateCertificate, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<KeyVaultConfigurationProvider>();
            configurationBuilder.Add(new KeyVaultConfigurationProvider(appClientId, vaultName, storeName, storeLocation, certificateThumbprint, validateCertificate, logger));
            return configurationBuilder;
        }

        /// <summary>
        ///  Load the config provider which reads shared secret configuration from key vault making use of cert loaded from My store name in the CurrentUser location
        /// </summary>
        /// <param name="configurationBuilder"></param>
        /// <param name="appClientId"></param>
        /// <param name="vaultName"></param>
        /// <param name="certificateThumbprint"></param>
        /// <param name="validateCertificate"></param>
        /// <returns></returns>
        public static IConfigurationBuilder AddKeyVaultSecrets(this IConfigurationBuilder configurationBuilder, string appClientId, string vaultName, string certificateThumbprint, bool validateCertificate, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<KeyVaultConfigurationProvider>();
            configurationBuilder.Add(new KeyVaultConfigurationProvider(appClientId, vaultName, certificateThumbprint, validateCertificate, logger));
            return configurationBuilder;
        }
    }
}
