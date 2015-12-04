using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Tailspin.Surveys.TokenStorage;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RedisConnectionServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisConnection(this IServiceCollection services, Action<RedisConnectionBuilder> setupAction)
        {
            var builder = new RedisConnectionBuilder();
            if (setupAction != null)
            {
                setupAction(builder);
            }

            services.AddSingleton<IConnectionMultiplexer>(factory => ConnectionMultiplexer.Connect(builder.Options));
            return services;
        }
    }
}
