// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MultiTenantSurveyApp.DAL.DTOs
{
    public class UserSurveysDTO
    {
        public IEnumerable<SurveySummaryDTO> Published { get; set; }
        public IEnumerable<SurveySummaryDTO> Own  { get; set; }
        public IEnumerable<SurveySummaryDTO> Contribute { get; set; }
    }
}
