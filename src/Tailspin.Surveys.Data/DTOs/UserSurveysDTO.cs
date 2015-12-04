// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Tailspin.Surveys.Data.DTOs
{
    public class UserSurveysDTO
    {
        public ICollection<SurveySummaryDTO> Published { get; set; }
        public ICollection<SurveySummaryDTO> Own  { get; set; }
        public ICollection<SurveySummaryDTO> Contribute { get; set; }
    }
}
