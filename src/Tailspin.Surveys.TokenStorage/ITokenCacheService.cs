// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Tailspin.Surveys.TokenStorage
{
    public interface ITokenCacheService
    {
        /// <summary>
        /// Returns an instance of <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCache"/>.
        /// </summary>
        /// <param name="claimsPrincipal">Current user's <see cref="System.Security.Claims.ClaimsPrincipal"/>.</param>
        /// <returns>An instance of <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCache"/>.</returns>
        Task<TokenCache> GetCacheAsync(ClaimsPrincipal claimsPrincipal);

        /// <summary>
        /// Clears the token cache.
        /// </summary>
        /// <param name="claimsPrincipal">Current user's <see cref="System.Security.Claims.ClaimsPrincipal"/>.</param>
        Task ClearCacheAsync(ClaimsPrincipal claimsPrincipal);
    }
}
