using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Tailspin.Surveys.TokenStorage.Redis
{
    /// <summary>
    /// Builder class used to configure an instance of <see cref="StackExchange.Redis.ConfigurationOptions"/>.
    /// </summary>
    public class RedisConnectionBuilder
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Tailspin.Surveys.TokenStorage.Redis.RedisConnectionBuilder"/>/
        /// </summary>
        public RedisConnectionBuilder()
            : this(new ConfigurationOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Tailspin.Surveys.TokenStorage.Redis.RedisConnectionBuilder"/>/
        /// </summary>
        /// <param name="options">A <see cref="StackExchange.Redis.ConfigurationOptions"/> instance used to configure this instance.</param>
        public RedisConnectionBuilder(ConfigurationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Options = options;
        }

        internal ConfigurationOptions Options { get; private set; }

        public RedisConnectionBuilder AddEndpoint(string hostAndPort)
        {
            if (string.IsNullOrWhiteSpace(hostAndPort))
            {
                throw new ArgumentException("hostAndPort cannot be null, empty, or only whitespace.");
            }

            Options.EndPoints.Add(hostAndPort);
            return this;
        }

        public RedisConnectionBuilder UsePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("password cannot be null, empty, or only whitespace.");
            }

            Options.Password = password;
            return this;
        }

        public RedisConnectionBuilder UseSsl(bool useSsl = true)
        {
            Options.Ssl = useSsl;
            return this;
        }
    }
}
