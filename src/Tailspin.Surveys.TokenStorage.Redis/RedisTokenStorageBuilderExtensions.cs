using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tailspin.Surveys.TokenStorage;
using Tailspin.Surveys.TokenStorage.Redis;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="Tailspin.Surveys.TokenStorage.TokenStorageBuilder"/> extension methods used to add support for Redis.
    /// </summary>
    public static class RedisTokenStorageBuilderExtensions
    {
        /// <summary>
        /// Adds support for <see cref="Tailspin.Surveys.TokenStorage.Redis.RedisTokenCacheService"/> to the <see cref="Tailspin.Surveys.TokenStorage.TokenStorageBuilder"/>.
        /// </summary>
        /// <param name="builder">An instance of <see cref="Tailspin.Surveys.TokenStorage.TokenStorageBuilder"/> used to configure support for access token storage.</param>
        /// <returns>The instance of <see cref="Tailspin.Surveys.TokenStorage.TokenStorageBuilder"/> passed to this method.</returns>
        public static TokenStorageBuilder UseRedisTokenStorageService(this TokenStorageBuilder builder)
        {
            return builder.AddTokenCacheService<RedisTokenCacheService>();
        }
    }
}
