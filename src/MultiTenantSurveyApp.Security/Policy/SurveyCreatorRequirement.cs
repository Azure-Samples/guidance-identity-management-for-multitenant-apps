// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Security.Claims;
using Microsoft.AspNet.Authorization;

namespace MultiTenantSurveyApp.Security.Policy
{
    public class SurveyCreatorRequirement : AuthorizationHandler<SurveyCreatorRequirement>, IAuthorizationRequirement
    {
        protected override void Handle(AuthorizationContext context, SurveyCreatorRequirement requirement)
        {
            if (context.User.HasClaim(ClaimTypes.Role, Roles.SurveyAdmin) || context.User.HasClaim(ClaimTypes.Role, Roles.SurveyCreator))
            {
                context.Succeed(requirement);
                return;
            }

            context.Fail();
        }
    }
}
