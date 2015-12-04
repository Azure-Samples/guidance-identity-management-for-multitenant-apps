// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Tailspin.Surveys.Web.Security
{
    public interface ITokenCacheService
    {
        Task<TokenCache> GetCacheAsync(string userObjectId, string clientId);
        Task ClearCacheAsync(string userObjectId, string clientId);
    }
}
