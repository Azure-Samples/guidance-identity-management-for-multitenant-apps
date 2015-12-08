// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Tailspin.Surveys.Common;
using Tailspin.Surveys.Data.DataModels;

namespace Tailspin.Surveys.Data.DataStore
{
    public interface ISurveyStore
    {
        Task<ICollection<Survey>> GetSurveysByOwnerAsync(int userId, int pageIndex = 0, int pageSize = Constants.DefaultPageSize);
        Task<ICollection<Survey>> GetSurveysByContributorAsync(int userId, int pageIndex = 0, int pageSize = Constants.DefaultPageSize);
        Task<Survey> GetSurveyAsync(int id);
        Task<Survey> UpdateSurveyAsync(Survey survey);
        Task<Survey> AddSurveyAsync(Survey survey);
        Task<Survey> DeleteSurveyAsync(Survey survey);
        Task<ICollection<Survey>> GetPublishedSurveysAsync(int pageIndex = 0, int pageSize = Constants.DefaultPageSize);
        Task<ICollection<Survey>> GetPublishedSurveysByOwnerAsync(int userId, int pageIndex = 0, int pageSize = Constants.DefaultPageSize);
        Task<Survey> PublishSurveyAsync(int id);
        Task<Survey> UnPublishSurveyAsync(int id);
        Task<ICollection<Survey>> GetPublishedSurveysByTenantAsync(int tenantId, int pageIndex = 0, int pageSize = Constants.DefaultPageSize);
        Task<ICollection<Survey>> GetUnPublishedSurveysByTenantAsync(int tenantId, int pageIndex = 0, int pageSize = Constants.DefaultPageSize);
    }
}
