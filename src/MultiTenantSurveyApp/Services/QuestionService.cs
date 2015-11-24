// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.OptionsModel;
using MultiTenantSurveyApp.Configuration;
using MultiTenantSurveyApp.DAL.DTOs;
using MultiTenantSurveyApp.Models;
using MultiTenantSurveyApp.Security;
using Newtonsoft.Json;

namespace MultiTenantSurveyApp.Services
{
    // This is the client for MultiTenantSurveyApp.WebAPI QuestionController
    // Note: If we used Swagger for the API definition, we could generate the client.
    // (see Azure API Apps) 
    // Note the MVC6 version of Swashbuckler is called "Ahoy" and is still in beta: https://github.com/domaindrivendev/Ahoy
     
    public class QuestionService: IQuestionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAccessTokenService _accessTokenService;
        private readonly HttpClient _httpClient;

        public QuestionService(HttpClientService factory, IHttpContextAccessor httpContextAccessor, IAccessTokenService accessTokenService)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = factory.GetHttpClient();
            _accessTokenService = accessTokenService;
        }

        public async Task<ApiResult<QuestionDTO>> GetQuestionAsync(int id)
        {
            var path = $"/questions/{id}";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await SendMessagewithBearerTokenAsync(requestMessage).ConfigureAwait(false);
            return await ApiResult<QuestionDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<QuestionDTO>> CreateQuestionAsync(QuestionDTO question)
        {
            var path = $"/surveys/{question.SurveyId}/questions";
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, path) {Content = CreateJsonContent(question)};
            var response = await SendMessagewithBearerTokenAsync(requestMessage).ConfigureAwait(false);
            return await ApiResult<QuestionDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<QuestionDTO>> UpdateQuestionAsync(QuestionDTO question)
        {
            var path = $"/questions/{question.Id}";
            var requestMessage = new HttpRequestMessage(HttpMethod.Put, path) {Content = CreateJsonContent(question)};
            var response = await SendMessagewithBearerTokenAsync(requestMessage).ConfigureAwait(false);
            return await ApiResult<QuestionDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult> DeleteQuestionAsync(int id)
        {
            var path = $"/questions/{id}";
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, path);
            var response = await SendMessagewithBearerTokenAsync(requestMessage).ConfigureAwait(false);
            return new ApiResult { Response = response };
        }

        private static HttpContent CreateJsonContent<T>(T item)
        {
            var json = JsonConvert.SerializeObject(item);
            return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }

        private async Task<HttpResponseMessage> SendMessagewithBearerTokenAsync(HttpRequestMessage requestMessage)
        {
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _accessTokenService.GetTokenForWebApiAsync(_httpContextAccessor.HttpContext.User).ConfigureAwait(false));
            return await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

        }
    }
}
