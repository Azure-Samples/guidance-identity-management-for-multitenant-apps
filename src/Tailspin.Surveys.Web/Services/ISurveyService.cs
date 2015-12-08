// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Tailspin.Surveys.Data.DataModels;
using Tailspin.Surveys.Data.DTOs;
using Tailspin.Surveys.Web.Models;

namespace Tailspin.Surveys.Web.Services
{
    /// <summary>
    /// This interface defines the CRUD operations for <see cref="Survey"/>s.
    /// This interface also defines operations related to publishing <see cref="Survey"/>s
    /// and adding and processing <see cref="ContributorRequest"/>s.
    /// </summary>
    public interface ISurveyService
    {
        Task<ApiResult<SurveyDTO>> GetSurveyAsync(int id);
        Task<ApiResult<UserSurveysDTO>> GetSurveysForUserAsync(int userId);
        Task<ApiResult<TenantSurveysDTO>> GetSurveysForTenantAsync(int tenantId);
        Task<ApiResult<SurveyDTO>> CreateSurveyAsync(SurveyDTO survey);
        Task<ApiResult<SurveyDTO>> UpdateSurveyAsync(SurveyDTO survey);
        Task<ApiResult<SurveyDTO>> DeleteSurveyAsync(int id);
        Task<ApiResult<ContributorsDTO>> GetSurveyContributorsAsync(int id);
        Task<ApiResult<IEnumerable<SurveyDTO>>> GetPublishedSurveysAsync();
        Task<ApiResult<SurveyDTO>> PublishSurveyAsync(int id);
        Task<ApiResult<SurveyDTO>> UnPublishSurveyAsync(int id);
        Task<ApiResult> ProcessPendingContributorRequestsAsync();
        Task<ApiResult> AddContributorRequestAsync(ContributorRequest contributorRequest);
    }
}