// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Tailspin.Surveys.Web.Security
{
    public interface ISurveysTokenService
    {
        /// <summary>
        /// This method retrieves the access token for the WebAPI resource that has previously
        /// been retrieved and cached. This method will fail if an access token for the WebAPI 
        /// resource has not been retrieved and cached. You can use the RequestAccessTokenAsync
        /// method to retrieve and cache access tokens.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> for the user to whom the access token belongs.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task{System.String}"/> with an access token as its result.</returns>
        Task<string> GetTokenForWebApiAsync(ClaimsPrincipal user);

        /// <summary>
        /// This method acquires an access token using an authorization code and ADAL. The access token is then cached
        /// in a <see cref="TokenCache"/> to be used later (by calls to GetTokenForWebApiAsync).
        /// </summary>
        /// <param name="claimsPrincipal">A <see cref="ClaimsPrincipal"/> for the signed in user</param>
        /// <param name="authorizationCode">a string authorization code obtained when the user signed in</param>
        /// <param name="redirectUri">The Uri of the application requesting the access token</param>
        /// <param name="resource">The resouce identifier of the target resource</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task{Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationResult}"/>.</returns>
        Task<AuthenticationResult> RequestTokenAsync(
            ClaimsPrincipal claimsPrincipal,
            string authorizationCode,
            string redirectUri,
            string resource);

        /// <summary>
        /// This method clears the user's <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCache"/>.
        /// </summary>
        /// <param name="claimsPrincipal">The <see cref="System.Security.Claims.ClaimsPrincipal"/> for the user</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/></returns>
        Task ClearCacheAsync(ClaimsPrincipal claimPrincipal);
    }
}
