// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Tailspin.Surveys.Security.Policy
{
    /// <summary>
    /// This class defines various operations that can be performed on a <see cref="Tailspin.Surveys.Data.DataModels.Survey"/>
    /// </summary>
    public class Operations
    {
        public static OperationAuthorizationRequirement Create = new OperationAuthorizationRequirement { Name = "Create" };
        public static OperationAuthorizationRequirement Read = new OperationAuthorizationRequirement { Name = "Read" };
        public static OperationAuthorizationRequirement Update = new OperationAuthorizationRequirement { Name = "Update" };
        public static OperationAuthorizationRequirement Delete = new OperationAuthorizationRequirement { Name = "Delete" };
        public static OperationAuthorizationRequirement Publish = new OperationAuthorizationRequirement { Name = "Publish" };
        public static OperationAuthorizationRequirement UnPublish = new OperationAuthorizationRequirement { Name = "UnPublish" };
    }
}
