// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using Tailspin.Surveys.Data.DataModels;

namespace Tailspin.Surveys.Data.DTOs
{
    public class QuestionDTO
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Question")]
        public string Text { get; set; }

        public QuestionType Type { get; set; }

        [Display(Name = "Answer Choices")]
        public string PossibleAnswers { get; set; }
        public int SurveyId { get; set; }

    }
}
