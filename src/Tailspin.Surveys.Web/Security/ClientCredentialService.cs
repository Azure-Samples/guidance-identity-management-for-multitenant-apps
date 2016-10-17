// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Tailspin.Surveys.Web.Configuration;

namespace Tailspin.Surveys.Web.Security
{
    /// <summary>
    /// Creates and caches an instance of <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential"/>.
    /// </summary>
    public class ClientCredentialService : ICredentialService
    {
        private AdalCredential _credential;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tailspin.Surveys.Web.Security.ClientCredentialService"/>.
        /// </summary>
        /// <param name="options">The current application configuration options.</param>
        public ClientCredentialService(IOptions<ConfigurationOptions> options)
        {
            var config = options?.Value?.AzureAd;
            _credential = new AdalCredential(new ClientCredential(config.ClientId, config.ClientSecret));
        }

        /// <summary>
        /// Returns the cached instance of <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential"/>.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> containing the cached <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential"/> as its result.</returns>
        public Task<AdalCredential> GetCredentialsAsync()
        {
            return Task.FromResult(_credential);
        }
    }
}
