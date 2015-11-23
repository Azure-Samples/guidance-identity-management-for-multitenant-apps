// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using MultiTenantSurveyApp.DAL.DataModels;
using MultiTenantSurveyApp.DAL.DTOs;
using MultiTenantSurveyApp.Models;

namespace MultiTenantSurveyApp.Services
{
    public interface ISurveyService
    {
        Task<ApiResult<SurveyDTO>> GetSurveyAsync(int id);
        Task<ApiResult<UserSurveysDTO>> GetSurveysForUserAsync(int userId);
        Task<ApiResult<TenantSurveysDTO>> GetSurveysForTenantAsync(string tenantId);
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