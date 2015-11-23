// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using MultiTenantSurveyApp.DAL.DataModels;

namespace MultiTenantSurveyApp.DAL.DataStore
{
    public interface ISurveyStore
    {
        Task<IEnumerable<Survey>> GetAllSurveysAsync();
        Task<IEnumerable<Survey>> GetSurveysByOwnerAsync(int userId);
        Task<IEnumerable<Survey>> GetSurveysByContributorAsync(int userId);
//        Task<IEnumerable<Survey>> GetSurveysByTenantAsync(string tenantId);
        Task<Survey> GetSurveyAsync(int id);
        Task<Survey> UpdateSurveyAsync(Survey survey);
        Task<Survey> AddSurveyAsync(Survey survey);
        Task<Survey> DeleteSurveyAsync(Survey survey);
        Task<IEnumerable<Survey>> GetPublishedSurveysAsync();
        Task<IEnumerable<Survey>> GetPublishedSurveysByOwnerAsync(int userId);
        Task<Survey> PublishSurveyAsync(int id);
        Task<Survey> UnPublishSurveyAsync(int id);
        Task<IEnumerable<Survey>> GetPublishedSurveysByTenantAsync(string tenantId);
        Task<IEnumerable<Survey>> GetUnPublishedSurveysByTenantAsync(string tenantId);
    }
}
