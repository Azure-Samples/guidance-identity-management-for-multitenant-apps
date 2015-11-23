// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using MultiTenantSurveyApp.DAL.DataModels;

namespace MultiTenantSurveyApp.DAL.DataStore
{
    public class SqlServerSurveyStore : ISurveyStore
    {
        private ApplicationDbContext dbContext { get; set; }

        public SqlServerSurveyStore(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Task<IEnumerable<Survey>> GetAllSurveysAsync()
        {
            return Task.FromResult(dbContext.Surveys.AsEnumerable());
        }

        public Task<IEnumerable<Survey>> GetSurveysByOwnerAsync(int userId)
        {
            return Task.FromResult(dbContext.Surveys.Where(s => s.OwnerId == userId && s.Published == false).AsEnumerable());
        }

        public Task<IEnumerable<Survey>> GetPublishedSurveysByOwnerAsync(int userId)
        {
            return Task.FromResult(dbContext.Surveys.Where(s => s.OwnerId == userId && s.Published == true).AsEnumerable());
        }

        public Task<IEnumerable<Survey>> GetSurveysByContributorAsync(int userId)
        {
            var surveys = dbContext.SurveyContributors.Include(sc => sc.Survey).Where(sc => sc.UserId == userId && sc.Survey.Published == false).Select(sc => sc.Survey).AsEnumerable();
            return Task.FromResult(surveys);

            // Not sure why, but the following was throwing "The EF.Property<T> method may only be used within LINQ queries"
            // return Task.FromResult(dbContext.Surveys.Include(s => s.Contributors).Where(s => s.Contributors.FirstOrDefault(c => c.UserId == userId) != null).AsEnumerable());
        }

        public Task<IEnumerable<Survey>> GetPublishedSurveysByTenantAsync(string tenantId)
        {
            return Task.FromResult(dbContext.Surveys.Where(s => s.TenantId == tenantId && s.Published == true).AsEnumerable());
        }
        public Task<IEnumerable<Survey>> GetUnPublishedSurveysByTenantAsync(string tenantId)
        {
            return Task.FromResult(dbContext.Surveys.Where(s => s.TenantId == tenantId && s.Published == false).AsEnumerable());
        }
        public Task<IEnumerable<Survey>> GetPublishedSurveysAsync()
        {
            return Task.FromResult(dbContext.Surveys.Where(s => s.Published == true).AsEnumerable());
        }
        public Task<Survey> GetSurveyAsync(int id)
        {
            return dbContext.Surveys
                .Include(survey => survey.Contributors)
                .ThenInclude(contrib => contrib.User)
                .Include(survey => survey.Questions)
                .SingleOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Survey> GetSurveyWithContributorsAsync(int id)
        {
            var survey = await dbContext.Surveys.Include(s => s.Contributors).ThenInclude(x => x.User).SingleOrDefaultAsync(s => s.Id == id);
            return survey;
        }


        public async Task<Survey> UpdateSurveyAsync(Survey survey)
        {
            dbContext.Surveys.Attach(survey);
            dbContext.Entry(survey).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();

            return survey;
        }
        public async Task<Survey> AddSurveyAsync(Survey survey)
        {
            dbContext.Surveys.Add(survey);
            await dbContext.SaveChangesAsync();
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
                    dbContext.SurveyContributors.Remove(contributor);
                }
                await dbContext.SaveChangesAsync();
            }

            if (survey.Questions != null)
            {
                foreach (var question in survey.Questions)
                {
                    Debug.WriteLine("Looping through questions");
                    dbContext.Questions.Remove(question);
                }
                await dbContext.SaveChangesAsync();
            }

            dbContext.Surveys.Remove(survey);
            await dbContext.SaveChangesAsync();
            return survey;
        }

        public async Task<Survey> PublishSurveyAsync(int id)
        {
            var dbResult = dbContext.Surveys.SingleOrDefaultAsync(s => s.Id == id);
            Survey survey = dbResult.Result;
            survey.Published = true;
            dbContext.Surveys.Attach(survey);
            dbContext.Entry(survey).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
            return survey;
        }
        public async Task<Survey> UnPublishSurveyAsync(int id)
        {
            var dbResult = dbContext.Surveys.SingleOrDefaultAsync(s => s.Id == id);
            Survey survey = dbResult.Result;
            survey.Published = false;
            dbContext.Surveys.Attach(survey);
            dbContext.Entry(survey).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();
            return survey;
        }
    }
}
