// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using MultiTenantSurveyApp.Configuration;
using MultiTenantSurveyApp.Logging;

namespace MultiTenantSurveyApp.Services
{
    public class HttpClientService
    {
        private readonly HttpClient _httpClient;

        public HttpClientService(IHttpContextAccessor contextAccessor, ILoggerFactory loggerFactory, IOptions<ConfigurationOptions> options)
        {
            var httpHandler = new HttpClientLogHandler(
                    new HttpClientHandler(),
                    loggerFactory.CreateLogger<HttpClientLogHandler>(),
                    contextAccessor);

            _httpClient = new HttpClient(httpHandler);

            var baseAddress = options?.Value?.AppSettings.WebApiUrl;
            if(!string.IsNullOrEmpty(baseAddress))
            {
                _httpClient.BaseAddress = new Uri(baseAddress);
            }
        }    
        
        public HttpClient GetHttpClient()
        {
            return _httpClient;
        }
    }
}
