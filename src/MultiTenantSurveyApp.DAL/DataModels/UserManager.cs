// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Data.Entity;

namespace MultiTenantSurveyApp.DAL.DataModels
{
    public class UserManager : IDisposable
    {
        private bool _disposed;
        private readonly ApplicationDbContext _dbContext;
        private readonly CancellationToken _cancellationToken;

        public UserManager(ApplicationDbContext dbContext, IHttpContextAccessor contextAccessor)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            _dbContext = dbContext;
            _cancellationToken = contextAccessor?.HttpContext?.RequestAborted ?? CancellationToken.None;
        }

        public virtual async Task CreateAsync(User user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _dbContext.Add(user);
            await _dbContext.SaveChangesAsync(_cancellationToken);
        }

        public async Task UpdateAsync(User user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _dbContext.Attach(user);
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            _dbContext.Update(user);
            await _dbContext.SaveChangesAsync(_cancellationToken);
        }

        public async Task<User> FindByObjectIdentifier(string objectIdentifier)
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(objectIdentifier))
            {
                throw new ArgumentException("objectIdentifier cannot be null, empty, or whitespace.");
            }

            return await _dbContext.Users
                .Where(u => u.ObjectId == objectIdentifier)
                .SingleOrDefaultAsync(_cancellationToken);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _dbContext.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
