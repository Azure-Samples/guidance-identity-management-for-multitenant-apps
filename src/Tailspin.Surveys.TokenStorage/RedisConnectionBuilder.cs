using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Tailspin.Surveys.TokenStorage
{
    public class RedisConnectionBuilder
    {
        public RedisConnectionBuilder()
            : this(new ConfigurationOptions())
        {
        }

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
