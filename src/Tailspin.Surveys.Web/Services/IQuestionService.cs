// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Tailspin.Surveys.Data.DTOs;
using Tailspin.Surveys.Web.Models;

namespace Tailspin.Surveys.Web.Services
{
    /// <summary>
    /// This interface defines the CRUD operations for <see cref="Tailspin.Surveys.Data.DataModels.Question"/>s
    /// </summary>
    public interface IQuestionService
    {
        Task<ApiResult<QuestionDTO>> GetQuestionAsync(int id);
        Task<ApiResult<QuestionDTO>> CreateQuestionAsync(QuestionDTO question);
        Task<ApiResult<QuestionDTO>> UpdateQuestionAsync(QuestionDTO question);
        Task<ApiResult> DeleteQuestionAsync(int id);
    }
}