// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using MultiTenantSurveyApp.DAL.DTOs;
using MultiTenantSurveyApp.Models;

namespace MultiTenantSurveyApp.Services
{
    public interface IQuestionService
    {
        Task<ApiResult<QuestionDTO>> GetQuestionAsync(int id);
        Task<ApiResult<QuestionDTO>> CreateQuestionAsync(QuestionDTO question);
        Task<ApiResult<QuestionDTO>> UpdateQuestionAsync(QuestionDTO question);
        Task<ApiResult> DeleteQuestionAsync(int id);
    }
}