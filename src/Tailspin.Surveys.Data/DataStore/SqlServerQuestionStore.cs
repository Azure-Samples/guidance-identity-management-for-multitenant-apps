// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Tailspin.Surveys.Data.DataModels;

namespace Tailspin.Surveys.Data.DataStore
{
    public class SqlServerQuestionStore : IQuestionStore
    {
        private ApplicationDbContext _dbContext;

        public SqlServerQuestionStore(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

       public async Task<Question> GetQuestionAsync(int id)
        {
            return await _dbContext.Questions
                .SingleOrDefaultAsync(q => q.Id == id);
        }

        public async Task<Question> AddQuestionAsync(Question question)
        {
            _dbContext.Questions.Add(question);
            await _dbContext.SaveChangesAsync();
            return question;
        }

        public async Task<Question> UpdateQuestionAsync(Question question)
        {
            _dbContext.Questions.Attach(question);
            _dbContext.Entry(question).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return question;
        }
        public async Task<Question> DeleteQuestionAsync(Question question)
        {
            _dbContext.Questions.Remove(question);
            await _dbContext.SaveChangesAsync();
            return question;
        }
    }
}
