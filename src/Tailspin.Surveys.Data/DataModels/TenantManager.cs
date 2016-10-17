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
    public class TenantManager : IDisposable
    {
        private bool _disposed;
        private ApplicationDbContext _dbContext;
        private readonly CancellationToken _cancellationToken;

        public TenantManager(ApplicationDbContext dbContext, IHttpContextAccessor contextAccessor)
        {
            Guard.ArgumentNotNull(dbContext, nameof(dbContext));
            _dbContext = dbContext;
            _cancellationToken = contextAccessor?.HttpContext?.RequestAborted ?? CancellationToken.None;
        }

        public virtual async Task CreateAsync(Tenant tenant)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(tenant, nameof(tenant));

            _dbContext.Add(tenant);
            await _dbContext
                .SaveChangesAsync(_cancellationToken)
                .ConfigureAwait(false);
        }

        public virtual async Task UpdateTenantAsync(Tenant tenant)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(tenant, nameof(tenant));

            _dbContext.Attach(tenant);
            tenant.ConcurrencyStamp = Guid.NewGuid().ToString();
            _dbContext.Update(tenant);
            await _dbContext
                .SaveChangesAsync(_cancellationToken)
                .ConfigureAwait(false);
        }

        public virtual async Task DeleteTenantAsync(Tenant tenant)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNull(tenant, nameof(tenant));

            _dbContext.Remove(tenant);
            await _dbContext
                .SaveChangesAsync(_cancellationToken)
                .ConfigureAwait(false);
        }

        public virtual async Task<Tenant> FindByIssuerValueAsync(string issuerValue)
        {
            ThrowIfDisposed();
            Guard.ArgumentNotNullOrWhiteSpace(issuerValue, nameof(issuerValue));

            return await _dbContext.Tenants
                .SingleOrDefaultAsync(t => t.IssuerValue == issuerValue, _cancellationToken)
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
