using System;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MultiTenantSurveyApp.Configuration;
using Microsoft.Extensions.OptionsModel;
using System.Security.Cryptography.X509Certificates;
using MultiTenantSurveyApp.Common;
using System.Threading.Tasks;

namespace MultiTenantSurveyApp.Security
{
    /// <summary>
    /// Creates and caches the ADAL ClientAssertionCertificate 
    /// This class exists for performance reasons when using asymmetric encryption with ADAL 
    /// It read certs from the store once and export it to a byte array representation and creates the ClientAssertionCertificate only once
    /// Register this as a singleton
    /// </summary>
    public class AppCredentialService : IAppCredentialService
    {
        private Lazy<Task<ClientAssertionCertificate>> _assertion = null;

        public AppCredentialService(IOptions<ConfigurationOptions> configOptions)
        {
            var aadOptions = configOptions.Value?.AzureAd;
            Guard.ArgumentNotNull(aadOptions, "configOptions.AzureAd");
            Guard.ArgumentNotNull(aadOptions.Asymmetric, "configOptions.AzureAd.Assymetric");

            _assertion = new Lazy<Task<ClientAssertionCertificate>>(() => 
                Task.Factory.StartNew(() =>
                {
                    return GetAsymmetricCredentials(
                            aadOptions.ClientId,
                            aadOptions.Asymmetric.StoreName,
                            aadOptions.Asymmetric.StoreLocation,
                            aadOptions.Asymmetric.CertificateThumbprint,
                            false);
                })
            );
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<ClientAssertionCertificate> GetAsymmetricCredentials()
        {
            return await _assertion.Value;
        }

        private ClientAssertionCertificate GetAsymmetricCredentials(string clientId, StoreName storeName, StoreLocation storeLocation, string certThumbprint, bool validationRequired)
        {
            X509Certificate2 cert = CertificateUtility.FindCertificateByThumbprint(storeName, storeLocation, certThumbprint, validationRequired);
            string password = null;
            var certBytes = CertificateUtility.ExportCertificateWithPrivateKey(cert, out password);
            return new ClientAssertionCertificate(clientId, certBytes, password);
        }
    }
}

