// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using MultiTenantSurveyApp.DAL.DataModels;

namespace MultiTenantSurveyApp.DAL.DTOs
{
    public class ContributorsDTO
    {
        public int SurveyId { get; set; }
        public IEnumerable<User> Contributors { get; set; }
    }
}
