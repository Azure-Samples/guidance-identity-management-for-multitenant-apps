// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace MultiTenantSurveyApp.DAL.DataModels
{
    public class SurveyContributor
    {
        public int SurveyId { get; set; }
        public int UserId { get; set; }

        // Navigation properties
        public Survey Survey { get; set; }
        public User User { get; set; }
    }
}
