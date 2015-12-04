// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Tailspin.Surveys.Web.Security
{
    /// <summary>
    /// Interface implemented by services that provide <see cref="Tailspin.Surveys.Web.Security.AdalCredential"/> instances.
    /// </summary>
    public interface ICredentialService
    {
        /// <summary>
        /// Gets the credential implemented in this service.
        /// </summary>
        /// <returns>An instance of <see cref="Tailspin.Surveys.Security.AdalCredential"/></returns>
        Task<AdalCredential> GetCredentialsAsync();
    }
}
