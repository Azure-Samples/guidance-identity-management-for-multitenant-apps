// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Authorization.Infrastructure;
using Microsoft.Extensions.Logging;
using MultiTenantSurveyApp.DAL.DataModels;

namespace MultiTenantSurveyApp.Security.Policy
{
    /// <summary>
    /// This <see cref="IAuthorizationHandler"/> validates that the signed in user is authorized
    /// to perform a specific action defined in <see cref="Operations"/>, on a specific instance of <see cref="Survey"/>.
    /// 
    /// To create a survey, you must be in the SurveyCreator role.
    /// To update a survey, you must be the owner or a contributor.
    /// To delete, publish, or unpublish a survey, you must be the owner.
    /// </summary>
    public class SurveyAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Survey>
    {
        static readonly Dictionary<OperationAuthorizationRequirement, Func<List<UserPermissionType>, bool>> ValidateUserPermissions
            = new Dictionary<OperationAuthorizationRequirement, Func<List<UserPermissionType>, bool>>

            {
                { Operations.Create, x => x.Contains(UserPermissionType.Creator) },

                { Operations.Read, x => x.Contains(UserPermissionType.Creator) ||
                                        x.Contains(UserPermissionType.Reader) ||
                                        x.Contains(UserPermissionType.Contributor) ||
                                        x.Contains(UserPermissionType.Owner) },

                { Operations.Update, x => x.Contains(UserPermissionType.Contributor) ||
                                        x.Contains(UserPermissionType.Owner) },

                { Operations.Delete, x => x.Contains(UserPermissionType.Owner) },

                { Operations.Publish, x => x.Contains(UserPermissionType.Owner) },

                { Operations.UnPublish, x => x.Contains(UserPermissionType.Owner) }
            };

        private enum UserPermissionType { Admin, Creator, Reader, Contributor, Owner };
        private readonly ILogger _logger;

        public SurveyAuthorizationHandler(ILogger logger)
        {
            _logger = logger;
        }

        protected override void Handle(AuthorizationContext context, OperationAuthorizationRequirement operation, Survey resource)
        {
            // if the survey is in the same tenant
            //      Add SURVEYADMIN/SURVEYCREATER/SURVEYREADER to permission set
            // Else if survey is in differnt tenant
            //      Add CONTRIBUTOR to permissionset if user is contributor on survey

            // if user is owner of the survey
            //      Add OWNER to the permission set
            var permissions = new List<UserPermissionType>();
            string userTenantId = context.User.GetTenantIdValue();
            int userId = ClaimsPrincipalExtensions.GetUserKey(context.User);
            string user = context.User.GetUserName();

            if (resource.TenantId == userTenantId)
            {

                if (context.User.HasClaim(ClaimTypes.Role, Roles.SurveyAdmin))
                {
                    context.Succeed(operation);
                    return;
                }

                if (context.User.HasClaim(ClaimTypes.Role, Roles.SurveyCreator))
                {
                    permissions.Add(UserPermissionType.Creator);
                }
                else
                {
                    permissions.Add(UserPermissionType.Reader);
                }

                if (resource.OwnerId == userId)
                {
                    permissions.Add(UserPermissionType.Owner);
                }
            }
            if (resource.Contributors != null && resource.Contributors.Any(x => x.UserId == userId))
            {
                permissions.Add(UserPermissionType.Contributor);
            }
            var permissionValues = permissions.Select(p => p.ToString());
            if (ValidateUserPermissions[operation](permissions))
            {
                _logger.ValidatePermissionsSucceeded(user, userTenantId, operation.Name, permissionValues);
                context.Succeed(operation);
            }
            else
            {
                _logger.ValidatePermissionsFailed(user, userTenantId, operation.Name, permissionValues);
                context.Fail();
            }
        }
    }
}
