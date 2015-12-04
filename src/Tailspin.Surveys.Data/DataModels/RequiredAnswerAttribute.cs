// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace Tailspin.Surveys.Data.DataModels
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RequiredAnswerAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var question = value as Question;

            if (question == null)
            {
                return base.IsValid(value);
            }

            if (question.Type == QuestionType.MultipleChoice)
            {
                return !string.IsNullOrEmpty(question.PossibleAnswers);
            }

            return true;
        }
    }
}