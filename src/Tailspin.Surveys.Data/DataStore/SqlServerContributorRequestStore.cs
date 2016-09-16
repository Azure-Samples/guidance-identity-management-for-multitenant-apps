// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tailspin.Surveys.Data.DataModels;

namespace Tailspin.Surveys.Data.DataStore
{
    public class SqlServerContributorRequestStore : IContributorRequestStore
    {
        private ApplicationDbContext _dbContext { get; set; }

        public SqlServerContributorRequestStore(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ICollection<ContributorRequest>> GetRequestForSurveyAsync(int surveyId)
        {
            return await _dbContext.ContributorRequests
                .Where(r => r.SurveyId == surveyId)
                .ToArrayAsync()
                .ConfigureAwait(false);
        }

        public async Task AddRequestAsync(ContributorRequest contributorRequest)
        {
            _dbContext.ContributorRequests.Add(contributorRequest);
            await _dbContext
                .SaveChangesAsync()
                .ConfigureAwait(false);
        }

        public async Task<ICollection<ContributorRequest>> GetRequestsForUserAsync(string emailAddress)
        {
            return await _dbContext.ContributorRequests
                .Where(c => c.EmailAddress.ToLower() == emailAddress.ToLower())
                .ToArrayAsync()
                .ConfigureAwait(false);
        }

        public async Task RemoveRequestAsync(ContributorRequest contributorRequest)
        {
            _dbContext.ContributorRequests.Remove(contributorRequest);
            await _dbContext
                .SaveChangesAsync()
                .ConfigureAwait(false);
        }
    }
}
