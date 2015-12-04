// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tailspin.Surveys.Common
{
    public static class HttpClientExtensions
    {
        /// <summary>
        /// This extension method for <see cref="HttpClient"/> provides a convenient overload that accepts 
        /// a <see cref="string"/> accessToken to be used as Bearer authentication.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> instance</param>
        /// <param name="method">The <see cref="HttpMethod"/></param>
        /// <param name="path">The path to the requested target</param>
        /// <param name="requestBody">The body of the request</param>
        /// <param name="accessToken">The access token to be used as Bearer authentication</param>
        /// <param name="ct">A <see cref="CancellationToken"/></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> SendRequestWithBearerTokenAsync(this HttpClient httpClient, HttpMethod method, string path, object requestBody, string accessToken, CancellationToken ct)
        {
            var request = new HttpRequestMessage(method, path);

            if (requestBody != null)
            {
                var json = JsonConvert.SerializeObject(requestBody, Formatting.None);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                request.Content = content;
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await httpClient.SendAsync(request, ct);
            return response;
        }
    }
}
