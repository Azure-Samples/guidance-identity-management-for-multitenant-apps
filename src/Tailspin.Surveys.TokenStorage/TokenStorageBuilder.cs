using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Tailspin.Surveys.Common;

namespace Tailspin.Surveys.TokenStorage
{
    /// <summary>
    /// Builder to configure token storage.
    /// </summary>
    public class TokenStorageBuilder
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Tailspin.Surveys.TokenStorage.TokenStorageBuilder"/>.
        /// </summary>
        /// <param name="services">The <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> configured for this application.</param>
        public TokenStorageBuilder(IServiceCollection services)
        {
            Guard.ArgumentNotNull(services, nameof(services));
            Services = services;
        }

        private IServiceCollection Services { get; set; }

        /// <summary>
        /// Replaces the currently configured <see cref="Tailspin.Surveys.TokenStorage.ITokenCacheService"/> implementation with the specialized implementation.
        /// </summary>
        /// <typeparam name="T">Type that implements <see cref="Tailspin.Surveys.TokenStorage.ITokenCacheService"/>.</typeparam>
        /// <returns>The current instance of <see cref="Tailspin.Surveys.TokenStorage.TokenStorageBuilder"/>.</returns>
        public TokenStorageBuilder AddTokenCacheService<T>()
            where T : class, ITokenCacheService
        {
            Services.AddScoped<ITokenCacheService, T>();
            return this;
        }
    }
}
