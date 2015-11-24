// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace MultiTenantSurveyApp.Security
{
    public interface IAppCredentialService
    {
        Task<ClientAssertionCertificate> GetAsymmetricCredentialsAsync();
    }
}
