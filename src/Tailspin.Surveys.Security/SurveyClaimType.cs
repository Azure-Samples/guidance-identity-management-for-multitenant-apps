// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Tailspin.Surveys.Security
{
    public static class SurveyClaimTypes
    {
        public const string SurveyUserIdClaimType = "survey_userid";
        public const string SurveyTenantIdClaimType = "survey_tenantid";
        public const string TenantId = "http://schemas.microsoft.com/identity/claims/tenantid";
        public const string ObjectId = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        public const string IssuerValue = "iss";
    }
}
