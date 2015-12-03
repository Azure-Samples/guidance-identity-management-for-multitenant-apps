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
    public class TenantManager : IDisposable
    {
        private bool _disposed;
        private ApplicationDbContext _dbContext;
        private readonly CancellationToken _cancellationToken;

        public TenantManager(ApplicationDbContext dbContext, IHttpContextAccessor contextAccessor)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            _dbContext = dbContext;
            _cancellationToken = contextAccessor?.HttpContext?.RequestAborted ?? CancellationToken.None;
        }

        public virtual async Task CreateAsync(Tenant tenant)
        {
            ThrowIfDisposed();
            if (tenant == null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            _dbContext.Add(tenant);
            await _dbContext.SaveChangesAsync(_cancellationToken);
        }

        public virtual async Task UpdateTenantAsync(Tenant tenant)
        {
            ThrowIfDisposed();
            if (tenant == null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            _dbContext.Attach(tenant);
            tenant.ConcurrencyStamp = Guid.NewGuid().ToString();
            _dbContext.Update(tenant);
            await RemoveUnclaimedEntriesAsync();
            await _dbContext.SaveChangesAsync(_cancellationToken);
        }

        public virtual async Task DeleteTenantAsync(Tenant tenant)
        {
            ThrowIfDisposed();
            if (tenant == null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            _dbContext.Remove(tenant);
            await _dbContext.SaveChangesAsync(_cancellationToken);
        }

        private async Task RemoveUnclaimedEntriesAsync()
        {
            // remove older, unclaimed entries
            DateTimeOffset tenMinutesAgo = DateTimeOffset.UtcNow.Subtract(new TimeSpan(0, 10, 0)); // workaround for Linq to entities
            var garbage = _dbContext.Tenants
                .Where(t => (!t.IssuerValue.StartsWith("https") && (t.Created < tenMinutesAgo)));
            await garbage.ForEachAsync(t => _dbContext.Remove(t), _cancellationToken);
        }

        public virtual async Task<Tenant> FindByIssuerValueAsync(string issuerValue)
        {
            ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(issuerValue))
            {
                throw new ArgumentException("issuerValue cannot be null, empty, or only whitespace.");
            }

            return await _dbContext.Tenants
                .SingleOrDefaultAsync(t => t.IssuerValue == issuerValue, _cancellationToken)
                .ConfigureAwait(false);
            //return await Store.FindByIssuerValueAsync(NormalizeKey(issuerValue), CancellationToken);
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
