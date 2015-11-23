// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MultiTenantSurveyApp.Security
{
    public static class SurveyClaimTypes
    {
        public const string SurveyUserIdClaimType = "survey_userid";
        public const string TenantId = "http://schemas.microsoft.com/identity/claims/tenantid";
        public const string ObjectId = "http://schemas.microsoft.com/identity/claims/objectidentifier";
    }
}
