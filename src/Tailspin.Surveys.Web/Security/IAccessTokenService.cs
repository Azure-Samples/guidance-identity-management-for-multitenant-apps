// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Tailspin.Surveys.Web.Security
{
    public interface IAccessTokenService
    {
        Task<string> GetTokenForWebApiAsync(ClaimsPrincipal user);
        Task<AuthenticationResult> RequestAccessTokenAsync(
            ClaimsPrincipal claimsPrincipal,
            string authorizationCode,
            string redirectUri,
            string resource);
        Task ClearCacheAsync(ClaimsPrincipal claimPrincipal);
    }
}
