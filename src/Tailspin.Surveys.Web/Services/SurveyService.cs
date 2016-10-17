// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tailspin.Surveys.Common;
using Tailspin.Surveys.Data.DataModels;
using Tailspin.Surveys.Data.DTOs;
using Tailspin.Surveys.Web.Models;
using Tailspin.Surveys.Web.Security;

namespace Tailspin.Surveys.Web.Services
{
    /// <summary>
    /// This is the client for Tailspin.Surveys.WebAPI SurveyController
    /// Note: If we used Swagger for the API definition, we could generate the client.
    /// (see Azure API Apps) 
    /// Note the MVC6 version of Swashbuckler is called "Ahoy" and is still in beta: https://github.com/domaindrivendev/Ahoy
    ///
    /// All methods except GetPublishedSurveysAsync set the user's access token in the Bearer authorization header 
    /// to allow the WebAPI to run on behalf of the signed in user.
    /// </summary>
    public class SurveyService : ISurveyService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISurveysTokenService _surveysTokenService;
        private readonly HttpClient _httpClient;
        private readonly CancellationToken _cancellationToken;


        public SurveyService(HttpClientService factory, IHttpContextAccessor httpContextAccessor, ISurveysTokenService surveysTokenService)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = factory.GetHttpClient();
            _surveysTokenService = surveysTokenService;
            _cancellationToken = httpContextAccessor?.HttpContext?.RequestAborted ?? CancellationToken.None;
        }

        public async Task<ApiResult<SurveyDTO>> GetSurveyAsync(int id)
        {
            var path = $"/surveys/{id}";
            var response =
                await _httpClient.SendRequestWithBearerTokenAsync(HttpMethod.Get, path, null,
                        await _surveysTokenService.GetTokenForWebApiAsync(_httpContextAccessor.HttpContext.User)
                                .ConfigureAwait(false), _cancellationToken);
            return await ApiResult<SurveyDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<UserSurveysDTO>> GetSurveysForUserAsync(int userId)
        {
            var path = $"/users/{userId}/surveys";
            var response = await _httpClient.SendRequestWithBearerTokenAsync(HttpMethod.Get, path, null,
                        await _surveysTokenService.GetTokenForWebApiAsync(_httpContextAccessor.HttpContext.User)
                                .ConfigureAwait(false), _cancellationToken);
            return await ApiResult<UserSurveysDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<TenantSurveysDTO>> GetSurveysForTenantAsync(int tenantId)
        {
            var path = $"/tenants/{tenantId}/surveys";
            var response = await _httpClient.SendRequestWithBearerTokenAsync(HttpMethod.Get, path, null,
                        await _surveysTokenService.GetTokenForWebApiAsync(_httpContextAccessor.HttpContext.User)
                                .ConfigureAwait(false), _cancellationToken);
            return await ApiResult<TenantSurveysDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }
        public async Task<ApiResult<IEnumerable<SurveyDTO>>> GetPublishedSurveysAsync()
        {
            var path = "/surveys/published";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            return await ApiResult<IEnumerable<SurveyDTO>>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<SurveyDTO>> CreateSurveyAsync(SurveyDTO survey)
        {
            var path = "/surveys";
            var response = await _httpClient.SendRequestWithBearerTokenAsync(HttpMethod.Post, path, survey,
                        await _surveysTokenService.GetTokenForWebApiAsync(_httpContextAccessor.HttpContext.User)
                                .ConfigureAwait(false), _cancellationToken);
            return await ApiResult<SurveyDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<SurveyDTO>> UpdateSurveyAsync(SurveyDTO survey)
        {
            var path = $"/surveys/{survey.Id}";
            var response = await _httpClient.SendRequestWithBearerTokenAsync(HttpMethod.Put, path, survey,
                        await _surveysTokenService.GetTokenForWebApiAsync(_httpContextAccessor.HttpContext.User)
                                .ConfigureAwait(false), _cancellationToken);
            return await ApiResult<SurveyDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<SurveyDTO>> DeleteSurveyAsync(int id)
        {
            var path = $"/surveys/{id}";
            var response = await _httpClient.SendRequestWithBearerTokenAsync(HttpMethod.Delete, path, null,
                        await _surveysTokenService.GetTokenForWebApiAsync(_httpContextAccessor.HttpContext.User)
                                .ConfigureAwait(false), _cancellationToken);
            return await ApiResult<SurveyDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }
        public async Task<ApiResult<SurveyDTO>> PublishSurveyAsync(int id)
        {
            var path = $"/surveys/{id}/publish";
            var response = await _httpClient.SendRequestWithBearerTokenAsync(HttpMethod.Put, path, null,
                        await _surveysTokenService.GetTokenForWebApiAsync(_httpContextAccessor.HttpContext.User)
                                .ConfigureAwait(false), _cancellationToken);
            return await ApiResult<SurveyDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }
        public async Task<ApiResult<SurveyDTO>> UnPublishSurveyAsync(int id)
        {
            var path = $"/surveys/{id}/unpublish";
            var response = await _httpClient.SendRequestWithBearerTokenAsync(HttpMethod.Put, path, null,
                        await _surveysTokenService.GetTokenForWebApiAsync(_httpContextAccessor.HttpContext.User)
                                .ConfigureAwait(false), _cancellationToken);
            return await ApiResult<SurveyDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<ContributorsDTO>> GetSurveyContributorsAsync(int id)
        {
            var path = $"/surveys/{id}/contributors";
            var response = await _httpClient.SendRequestWithBearerTokenAsync(HttpMethod.Get, path, null,
                        await _surveysTokenService.GetTokenForWebApiAsync(_httpContextAccessor.HttpContext.User)
                                .ConfigureAwait(false), _cancellationToken);
            return await ApiResult<ContributorsDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult> ProcessPendingContributorRequestsAsync()
        {
            var path = $"/surveys/processpendingcontributorrequests";
            var response = await _httpClient.SendRequestWithBearerTokenAsync(HttpMethod.Post, path, null,
                        await _surveysTokenService.GetTokenForWebApiAsync(_httpContextAccessor.HttpContext.User)
                                .ConfigureAwait(false), _cancellationToken);
            return new ApiResult { Response = response };
        }

        public async Task<ApiResult> AddContributorRequestAsync(ContributorRequest contributorRequest)
        {
            var path = $"/surveys/{contributorRequest.SurveyId}/contributorrequests";
            var response = await _httpClient.SendRequestWithBearerTokenAsync(HttpMethod.Post, path, contributorRequest,
                        await _surveysTokenService.GetTokenForWebApiAsync(_httpContextAccessor.HttpContext.User)
                                .ConfigureAwait(false), _cancellationToken);
            return new ApiResult { Response = response };
        }
    }
}
