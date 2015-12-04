// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Abstractions;
using Microsoft.AspNet.Routing;
using Moq;
using Tailspin.Surveys.Data.DataModels;
using Tailspin.Surveys.Data.DataStore;
using Tailspin.Surveys.Data.DTOs;
using Tailspin.Surveys.Security;
using Tailspin.Surveys.WebAPI.Controllers;
using Xunit;

namespace MultiTentantSurveyAppTests
{
    public class WebAPISurveyControllerTests
    {
        private Mock<ISurveyStore> _surveyStore;
        private Mock<IContributorRequestStore> _contributorRequestStore;
        private Mock<IAuthorizationService> _authorizationService;
        private SurveyController _target;

        public WebAPISurveyControllerTests()
        {
            _surveyStore = new Mock<ISurveyStore>();
            _contributorRequestStore = new Mock<IContributorRequestStore>();
            _authorizationService = new Mock<IAuthorizationService>();
            _target = new SurveyController(_surveyStore.Object, _contributorRequestStore.Object, _authorizationService.Object);
        }

        [Fact]
        public async Task GetSurveysForUser_ReturnsSurveys()
        {
            _surveyStore.Setup(s => s.GetPublishedSurveysByOwnerAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Survey>());
            _surveyStore.Setup(s => s.GetSurveysByOwnerAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Survey>());
            _surveyStore.Setup(s => s.GetSurveysByContributorAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Survey>());

            _target.ActionContext = CreateActionContextWithUserPrincipal("12345", "testTenantId");
            var result = await _target.GetSurveysForUser(12345);

            var objectResult = (ObjectResult) result;
            Assert.IsType<UserSurveysDTO>(objectResult.Value);
        }

        [Fact]
        public async Task GetSurveysForUser_FailsIfNotUser()
        {
            _target.ActionContext = CreateActionContextWithUserPrincipal("00000", "testTenantId");
            var result = await _target.GetSurveysForUser(12345);

            var statusCodeResult = (HttpStatusCodeResult)result;
            Assert.Equal(403, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task GetSurveysForTenant_ReturnsSurveys()
        {
            _surveyStore.Setup(s => s.GetPublishedSurveysByTenantAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Survey>());
            _surveyStore.Setup(s => s.GetUnPublishedSurveysByTenantAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Survey>());

            _target.ActionContext = CreateActionContextWithUserPrincipal("12345", "testTenantId");
            var result = await _target.GetSurveysForTenant("testTenantId");

            var objectResult = (ObjectResult)result;
            Assert.IsType<TenantSurveysDTO>(objectResult.Value);
        }

        [Fact]
        public async Task GetSurveysForTenant_FailsIfNotInSameTenant()
        {
            _target.ActionContext = CreateActionContextWithUserPrincipal("12345", "otherTenantId");
            var result = await _target.GetSurveysForTenant("testTenantId");

            var statusCodeResult = (HttpStatusCodeResult)result;
            Assert.Equal(403, statusCodeResult.StatusCode);
        }

        private ActionContext CreateActionContextWithUserPrincipal(string userId, string tenantId)
        {
            var httpContext = new Mock<HttpContext>();
            var routeData = new Mock<RouteData>();
            var actionDescriptor = new Mock<ActionDescriptor>();
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(SurveyClaimTypes.SurveyUserIdClaimType, userId),
                new Claim(SurveyClaimTypes.TenantId, tenantId)

            }));
            httpContext.SetupGet(c => c.User).Returns(principal);
            return new ActionContext(httpContext.Object, routeData.Object, actionDescriptor.Object);
        }
    }
}
