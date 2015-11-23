// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using MultiTenantSurveyApp.DAL.DataModels;

namespace MultiTenantSurveyApp.DAL.DataStore
{
    public class SqlServerQuestionStore : IQuestionStore
    {
        private ApplicationDbContext dbContext { get; set; }

        public SqlServerQuestionStore(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IEnumerable<Question> GetQuestionsForSurvey(int surveyId)
        {
            return dbContext.Questions
                .Where(q => q.SurveyId == surveyId);
        }
        public async Task<Question> GetQuestionAsync(int id)
        {
            return await dbContext.Questions
                .SingleOrDefaultAsync(q => q.Id == id);
        }

        public async Task<Question> AddQuestionAsync(Question question)
        {
            dbContext.Questions.Add(question);
            await dbContext.SaveChangesAsync();
            return question;
        }

        public async Task<Question> UpdateQuestionAsync(Question question)
        {
            dbContext.Questions.Attach(question);
            dbContext.Entry(question).State = EntityState.Modified;
            await dbContext.SaveChangesAsync();

            return question;
        }
        public async Task<Question> DeleteQuestionAsync(Question question)
        {
            dbContext.Questions.Remove(question);
            await dbContext.SaveChangesAsync();
            return question;
        }
    }
}
