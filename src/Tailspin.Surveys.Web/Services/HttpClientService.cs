// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tailspin.Surveys.Web.Configuration;
using Tailspin.Surveys.Web.Logging;

namespace Tailspin.Surveys.Web.Services
{
    /// <summary>
    /// This class wraps an instance of HttpClient configured with a handler 
    /// that performs logging.
    /// </summary>
    public class HttpClientService
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// This constructor configures the base address of the <see cref="HttpClient"/> instance member with the 
        /// value of the WebApiUrl if this value is found in configuration. This constructor also configures the 
        /// <see cref="HttpClient"/> instance member with an instance of <see cref="HttpClientLogHandler"/> 
        /// which requires an <see cref="IHttpContextAccessor"/> and a <see cref="ILoggerFactory"/>.
        /// </summary>
        /// <param name="contextAccessor"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="options"></param>
        public HttpClientService(IHttpContextAccessor contextAccessor, ILoggerFactory loggerFactory, IOptions<ConfigurationOptions> options)
        {
            var httpHandler = new HttpClientLogHandler(
                    new HttpClientHandler(),
                    loggerFactory.CreateLogger<HttpClientLogHandler>(),
                    contextAccessor);

            _httpClient = new HttpClient(httpHandler);

            // Set the BaseAddress of the HttpClient if WebApiUrl is found in configuration.
            var baseAddress = options?.Value?.AppSettings.WebApiUrl;
            if(!string.IsNullOrEmpty(baseAddress))
            {
                _httpClient.BaseAddress = new Uri(baseAddress);
            }
        }    
        
        /// <summary>
        /// This method exposes the configured <see cref="HttpClient"/> member.
        /// </summary>
        /// <returns>An instance of <see cref="HttpClient"/> configured to perform logging</returns>
        public HttpClient GetHttpClient()
        {
            return _httpClient;
        }
    }
}
