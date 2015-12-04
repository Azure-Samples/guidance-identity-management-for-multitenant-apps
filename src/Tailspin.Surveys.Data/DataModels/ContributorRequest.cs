// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace Tailspin.Surveys.Data.DataModels
{
    public class ContributorRequest
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int SurveyId { get; set; }

        [Required]
        public string EmailAddress { get; set; }

        [Required]
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    }
}
