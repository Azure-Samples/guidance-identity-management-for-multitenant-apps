using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Tailspin.Surveys.TokenStorage;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TokenStorageServiceCollectionExtensions
    {
        public static TokenStorageBuilder AddTokenStorage(this IServiceCollection services)
        {
            // Add the default token service
            services.AddScoped<ITokenCacheService, DefaultTokenCacheService>();
            return new TokenStorageBuilder(services);
        }
    }
}
