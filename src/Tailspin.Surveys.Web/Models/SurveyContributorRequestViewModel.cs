// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;

namespace Tailspin.Surveys.Web.Models
{
    public class SurveyContributorRequestViewModel
    {
        [Required]
        public int SurveyId { get; set; }

        [Required]
        public string EmailAddress { get; set; }
    }
}
