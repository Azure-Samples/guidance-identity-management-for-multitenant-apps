// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Tailspin.Surveys.Data.DataModels;

namespace Tailspin.Surveys.Data.DTOs
{
    public class DataMapping
    {
        public static readonly Func<Question, QuestionDTO> _questionToDto = x => new QuestionDTO
        {
            Id = x.Id,
            SurveyId = x.SurveyId,
            Text = x.Text,
            Type = x.Type,
            PossibleAnswers = x.PossibleAnswers
        };

        public static readonly Func<Survey, SurveyDTO> _surveyToDto = x =>
        {
            var surveyDto = new SurveyDTO
            {
                Id = x.Id,
                OwnerId = x.OwnerId,
                Title = x.Title,
                Published = x.Published,
            };

            if (x.Questions != null)
            {
                surveyDto.Questions = x.Questions.Select(QuestionToDto).ToArray();
            }
            else
            {
                surveyDto.Questions = new QuestionDTO[] { };
            }

            return surveyDto;
        };

        public static Func<Survey, SurveySummaryDTO> _surveyToSummaryDto = x =>
        {
            var surveySummaryDto = new SurveySummaryDTO
            {
                Id = x.Id,
                Title = x.Title
            };

            return surveySummaryDto;
        };

        public static readonly Func<Question, QuestionDTO> QuestionToDto = x => new QuestionDTO
        {
            Id = x.Id,
            SurveyId = x.SurveyId,
            Text = x.Text,
            Type = x.Type,
            PossibleAnswers = x.PossibleAnswers
        };

        public static readonly Func<SurveyDTO, Survey> _dtoToSurvey = x => new Survey { Id = x.Id, Title = x.Title, Published = x.Published };
    }
}
