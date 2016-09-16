// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Tailspin.Surveys.Common;

namespace Tailspin.Surveys.Data.DataModels
{
    public class UserManager : IDisposable
    {
        private bool _disposed;
        private readonly ApplicationDbContext _dbContext;
        private readonly CancellationToken _cancellationToken;

        public UserManager(ApplicationDbContext dbContext, IHttpContextAccessor contextAccessor)
        {
            Guard.ArgumentNotNull(dbContext, nameof(dbContext));

            _dbContext = dbContext;
            _cancellationToken = contextAccessor?.HttpContext?.RequestAborted ?? CancellationToken.None;
        }

        public virtual async Task CreateAsync(User user)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));

            _dbContext.Add(user);
            await _dbContext
                .SaveChangesAsync(_cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task UpdateAsync(User user)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(user, nameof(user));

            _dbContext.Attach(user);
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            _dbContext.Update(user);
            await _dbContext
                .SaveChangesAsync(_cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<User> FindByObjectIdentifier(string objectIdentifier)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNullOrWhiteSpace(objectIdentifier, nameof(objectIdentifier));

            return await _dbContext.Users
                .Where(u => u.ObjectId == objectIdentifier)
                .SingleOrDefaultAsync(_cancellationToken)
                .ConfigureAwait(false);
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
