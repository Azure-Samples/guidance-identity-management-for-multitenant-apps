// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Tailspin.Surveys.Security.Policy
{
    /// <summary>
    /// This <see cref="IAuthorizationRequirement"/> functions as an <see cref="IAuthorizationHandler"/> that
    /// validates that the signed in user has either SurveyCreator or SurveyAdmin role claim.
    /// </summary>
    public class SurveyCreatorRequirement : AuthorizationHandler<SurveyCreatorRequirement>, IAuthorizationRequirement
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SurveyCreatorRequirement requirement)
        {
            if (context.User.HasClaim(ClaimTypes.Role, Roles.SurveyAdmin) || context.User.HasClaim(ClaimTypes.Role, Roles.SurveyCreator))
            {
                context.Succeed(requirement);
            }

            return Task.FromResult(0);
        }
    }
}
