// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.Extensions.OptionsModel;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Tailspin.Surveys.Web.Configuration;

namespace Tailspin.Surveys.Web.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class ClientCredentialService : ICredentialService
    {
        private AdalCredential _credential;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public ClientCredentialService(IOptions<ConfigurationOptions> options)
        {
            var config = options?.Value?.AzureAd;
            _credential = new AdalCredential(new ClientCredential(config.ClientId, config.ClientSecret));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<AdalCredential> GetCredentialsAsync()
        {
            return Task.FromResult(_credential);
        }
    }
}
