// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using MultiTenantSurveyApp.Security;
using System.Diagnostics;

namespace MultiTenantSurveyApp.Logging
{
    internal class HttpClientLogHandler : DelegatingHandler
    {
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpClientLogHandler(HttpMessageHandler innerHandler, ILogger logger, IHttpContextAccessor contextAccessor) : base(innerHandler)
        {
            _logger = logger;
            _httpContextAccessor = contextAccessor;
        }

        // [masimms-roshar] What does this method do?
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            // Get the user Id and tenant Id from signed-in user's ClaimsPrincipal
            var userId = _httpContextAccessor?.HttpContext.User.FindFirstValue(SurveyClaimTypes.ObjectId) ?? "Anonymous";
            var tenantId = _httpContextAccessor?.HttpContext.User.FindFirstValue(SurveyClaimTypes.TenantId) ?? "-";

            var method = request.Method?.Method;
            var uri = request.RequestUri.AbsoluteUri;

            var requestStopwatch = Stopwatch.StartNew();
            var response = await base.SendAsync(request, cancellationToken);
            requestStopwatch.Stop();

            if (response.IsSuccessStatusCode)
            {
                _logger.RequestSucceeded(method, uri, requestStopwatch.Elapsed, userId, tenantId);
            }
            else
            {
                _logger.RequestFailed(method, uri, requestStopwatch.Elapsed, response.ReasonPhrase, response.StatusCode.ToString(), userId, tenantId);

            }
            return response;
        }
    }
}
