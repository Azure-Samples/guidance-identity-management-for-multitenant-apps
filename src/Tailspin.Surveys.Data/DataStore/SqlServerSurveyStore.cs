// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Tailspin.Surveys.Common;
using Tailspin.Surveys.Data.DataModels;

namespace Tailspin.Surveys.Data.DataStore
{
    public class SqlServerSurveyStore : ISurveyStore
    {
        private ApplicationDbContext _dbContext;

        public SqlServerSurveyStore(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<Survey>> GetSurveysByOwnerAsync(int userId, int pageIndex = 0, int pageSize = Constants.DefaultPageSize)
        {
            var cappedPageSize = Math.Min(Constants.MaxPageSize, pageSize);
            return await _dbContext.Surveys.Where(s => s.OwnerId == userId && s.Published == false)
                                     .OrderBy(s => s.Id)
                                     .Skip(pageIndex * cappedPageSize)
                                     .Take(cappedPageSize)
                                     .ToArrayAsync();
        }

        public async Task<ICollection<Survey>> GetPublishedSurveysByOwnerAsync(int userId, int pageIndex = 0, int pageSize = Constants.DefaultPageSize)
        {
            var cappedPageSize = Math.Min(Constants.MaxPageSize, pageSize);
            return await _dbContext.Surveys.Where(s => s.OwnerId == userId && s.Published)
                                           .OrderBy(s => s.Id)
                                           .Skip(pageIndex * cappedPageSize)
                                           .Take(cappedPageSize)
                                           .ToArrayAsync();
        }

        public async Task<ICollection<Survey>> GetSurveysByContributorAsync(int userId, int pageIndex = 0, int pageSize = Constants.DefaultPageSize)
        {
            var cappedPageSize = Math.Min(Constants.MaxPageSize, pageSize);
            return await _dbContext.SurveyContributors.Include(sc => sc.Survey)
                                                      .Where(sc => sc.UserId == userId && sc.Survey.Published == false)
                                                      .Select(sc => sc.Survey)
                                                      .OrderBy(s => s.Id)
                                                      .Skip(pageIndex * cappedPageSize)
                                                      .Take(cappedPageSize)
                                                      .ToArrayAsync();
        }

        public async Task<ICollection<Survey>> GetPublishedSurveysByTenantAsync(string tenantId, int pageIndex = 0, int pageSize = Constants.DefaultPageSize)
        {
            var cappedPageSize = Math.Min(Constants.MaxPageSize, pageSize);
            return await _dbContext.Surveys.Where(s => s.TenantId == tenantId && s.Published)
                                           .OrderBy(s => s.Id)
                                           .Skip(pageIndex * cappedPageSize)
                                           .Take(cappedPageSize)
                                           .ToArrayAsync();
        }

        public async Task<ICollection<Survey>> GetUnPublishedSurveysByTenantAsync(string tenantId, int pageIndex = 0, int pageSize = Constants.DefaultPageSize)
        {
            var cappedPageSize = Math.Min(Constants.MaxPageSize, pageSize);
            return await _dbContext.Surveys.Where(s => s.TenantId == tenantId && s.Published == false)
                                           .OrderBy(s => s.Id)
                                           .Skip(pageIndex * cappedPageSize)
                                           .Take(cappedPageSize)
                                           .ToArrayAsync();
        }

        public async Task<ICollection<Survey>> GetPublishedSurveysAsync(int pageIndex = 0, int pageSize = Constants.DefaultPageSize)
        {
            var cappedPageSize = Math.Min(Constants.MaxPageSize, pageSize);
            return await _dbContext.Surveys.Where(s => s.Published)
                                           .OrderBy(s => s.Id)
                                           .Skip(pageIndex * cappedPageSize)
                                           .Take(cappedPageSize)
                                           .ToArrayAsync();
        }

        public Task<Survey> GetSurveyAsync(int id)
        {
            return _dbContext.Surveys
                .Include(survey => survey.Contributors)
                .ThenInclude(contrib => contrib.User)
                .Include(survey => survey.Questions)
                .Include(survey => survey.Requests)
                .SingleOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Survey> GetSurveyWithContributorsAsync(int id)
        {
            var survey = await _dbContext.Surveys.Include(s => s.Contributors).ThenInclude(x => x.User).SingleOrDefaultAsync(s => s.Id == id);
            return survey;
        }

        public async Task<Survey> UpdateSurveyAsync(Survey survey)
        {
            _dbContext.Surveys.Attach(survey);
            _dbContext.Entry(survey).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return survey;
        }

        public async Task<Survey> AddSurveyAsync(Survey survey)
        {
            _dbContext.Surveys.Add(survey);
            await _dbContext.SaveChangesAsync();
            return survey;
        }

        public async Task<Survey> DeleteSurveyAsync(Survey survey)
        {
            // TODO: Not sure about the status of cascading deletes in EF7
            if (survey.Contributors != null)
            {
                foreach (var contributor in survey.Contributors)
                {
                    Debug.WriteLine("Looping through contributors");
                    _dbContext.SurveyContributors.Remove(contributor);
                }
                await _dbContext.SaveChangesAsync();
            }

            if (survey.Questions != null)
            {
                foreach (var question in survey.Questions)
                {
                    Debug.WriteLine("Looping through questions");
                    _dbContext.Questions.Remove(question);
                }
                await _dbContext.SaveChangesAsync();
            }

            _dbContext.Surveys.Remove(survey);
            await _dbContext.SaveChangesAsync();
            return survey;
        }

        public async Task<Survey> PublishSurveyAsync(int id)
        {
            var dbResult = _dbContext.Surveys.SingleOrDefaultAsync(s => s.Id == id);
            Survey survey = dbResult.Result;
            survey.Published = true;
            _dbContext.Surveys.Attach(survey);
            _dbContext.Entry(survey).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return survey;
        }

        public async Task<Survey> UnPublishSurveyAsync(int id)
        {
            var dbResult = _dbContext.Surveys.SingleOrDefaultAsync(s => s.Id == id);
            Survey survey = dbResult.Result;
            survey.Published = false;
            _dbContext.Surveys.Attach(survey);
            _dbContext.Entry(survey).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
            return survey;
        }
    }
}
