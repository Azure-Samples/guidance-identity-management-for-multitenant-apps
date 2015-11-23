// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using MultiTenantSurveyApp.DAL.DataModels;
using MultiTenantSurveyApp.DAL.DataStore;
using MultiTenantSurveyApp.DAL.DTOs;
using MultiTenantSurveyApp.Security.Policy;

namespace MultiTenantSurveyApp.WebAPI.Controllers
{
    /// <summary>
    /// This class provides a REST based API for the management of surveys.
    /// This class uses Bearer token authentication and authorization.
    /// </summary>
    [Authorize(ActiveAuthenticationSchemes = "Bearer")]
    public class SurveyController : Controller
    {
        private readonly ISurveyStore _surveyStore;
        private readonly IContributorRequestStore _contributorRequestStore;
        private readonly IAuthorizationService _authorizationService;

        public SurveyController(ISurveyStore surveyStore, IContributorRequestStore contributorRequestStore, IAuthorizationService authorizationService)
        {
            _surveyStore = surveyStore;
            _contributorRequestStore = contributorRequestStore;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// This method returns a Survey if one is found with a matching id property. 
        /// </summary>
        /// <param name="id">The id of the Survey</param>
        /// <returns>An ActionResult that contains a SurveyDTO if found, otherwise a Not Found response</returns>
        [HttpGet("surveys/{id:int}", Name = "GetSurvey")]
        public async Task<IActionResult> Get(int id)
        {
            var survey = await _surveyStore.GetSurveyAsync(id);
            if (survey == null)
            {
                //Logger.LogInformation("Details: Item not found {0}", id);
                return HttpNotFound();
            }

            // The AuthorizationService uses the policies in the MultiTenantSurveyApp.Security project
            if (await _authorizationService.AuthorizeAsync(User, survey, Operations.Read) == false)
            {
                return new HttpStatusCodeResult(403);
            }
            return new ObjectResult(DataMapping._surveyToDto(survey));
        }

        /// <summary>
        /// This method returns Surveys associated to the user: Published, Owned, or Contributed.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns>An ActionResult that contains a UserSurveysDTO populated with surveys that the user owns or contributes to, and published surveys</returns>
        [HttpGet("users/{userId}/surveys")]
        public async Task<IActionResult> GetSurveysForUser(int userId)
        {
            if (User.GetUserKey() != userId)
            {
                return new HttpUnauthorizedResult();
            }

            var surveys = new UserSurveysDTO();
            surveys.Published = (await _surveyStore.GetPublishedSurveysByOwnerAsync(userId)).Select(DataMapping._surveyToSummaryDto);
            surveys.Own = (await _surveyStore.GetSurveysByOwnerAsync(userId)).Select(DataMapping._surveyToSummaryDto);
            surveys.Contribute = (await _surveyStore.GetSurveysByContributorAsync(userId)).Select(DataMapping._surveyToSummaryDto);

            return new ObjectResult(surveys);
        }

        /// <summary>
        /// This method returns all surveys owned by users of a specific tenant.
        /// </summary>
        /// <param name="tenantId">The id of the tenant</param>
        /// <returns>An ActionResult that contains a TenantSurveysDTO populated with Published and Unpublished surveys associated with a tenant</returns>
        [HttpGet("tenants/{tenantId}/surveys")]
        public async Task<IActionResult> GetSurveysForTenant(string tenantId)
        {
            var surveys = new TenantSurveysDTO();
            surveys.Published = (await _surveyStore.GetPublishedSurveysByTenantAsync(tenantId)).Select(DataMapping._surveyToSummaryDto);
            surveys.UnPublished = (await _surveyStore.GetUnPublishedSurveysByTenantAsync(tenantId)).Select(DataMapping._surveyToSummaryDto);
            return new ObjectResult(surveys);
        }

        /// <summary>
        /// This method returns all published surveys. This method is anonymously accessible.
        /// </summary>
        /// <returns>An ActionResult that contains an enumarable collection of published surveys</returns>
        [HttpGet("surveys/published")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublishedSurveys()
        {
            var surveys = (await _surveyStore.GetPublishedSurveysAsync()).Select(DataMapping._surveyToDto);
            return new ObjectResult(surveys);
        }

        /// <summary>
        /// This method returns the contributors to a Survey.
        /// </summary>
        /// <param name="id">The id of the Survey</param>
        /// <returns>An ActionResult that contains a ContributorDTO populated with an enumerable collection of users who contribute to the specified Survey</returns>
        [HttpGet("surveys/{id}/contributors")]
        public async Task<IActionResult> GetSurveyContributors(int id)
        {
            var survey = await _surveyStore.GetSurveyAsync(id);
            if (survey == null)
            {
                return HttpNotFound();
            }

            // Validate that the current user has Read permissions to this survey.
            if (!await _authorizationService.AuthorizeAsync(User, survey, Operations.Read))
            {
                return new HttpUnauthorizedResult();
            }

            return new ObjectResult(new ContributorsDTO()
            {
                SurveyId = id,
                Contributors = survey.Contributors.Select(x => x.User)
            });
        }

        /// <summary>
        /// This method attempts to persist a new Survey using the values in the SurveyDTO instance.
        /// This action is decorated with an authorization policy that requires only users in the Survey Creator role can call it.
        /// </summary>
        /// <param name="item">An SurveyDTO that contains the values used to create and persist a new Survey</param>
        /// <returns>A CreatedAtRouteResult of the newly created Survey if successfully persisted</returns>
        [HttpPost("surveys")]
        [Authorize(Policy = PolicyNames.RequireSurveyCreator, ActiveAuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> Create([FromBody] SurveyDTO item)
        {
            if (item == null)
            {
                return HttpBadRequest();
            }
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            var survey = DataMapping._dtoToSurvey(item);
            survey.OwnerId = User.GetUserKey();
            survey.TenantId = User.GetTenantIdValue();

            await _surveyStore.AddSurveyAsync(survey);

            item.Id = survey.Id;

            return CreatedAtRoute("GetSurvey", new { controller = "Surveys", id = survey.Id }, item);
        }

        /// <summary>
        /// This method attempts to update the Survey with the specified id value.
        /// </summary>
        /// <param name="id">The id of the Survey</param>
        /// <param name="item">A SurveyDTO containing property values of the Survey</param>
        /// <returns>An ActionResult that contains a SurveyDTO containing property values of the updated Survey</returns>
        [HttpPut("surveys/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SurveyDTO item)
        {
            if (item == null || item.Id != id)
            {
                return HttpBadRequest();
            }
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            var survey = await _surveyStore.GetSurveyAsync(id);
            if (survey == null)
            {
                return HttpNotFound();
            }

            // Validate that the current user has Update permissions to this survey.
            if (!await _authorizationService.AuthorizeAsync(User, survey, Operations.Update))
            {
                return new HttpStatusCodeResult(403);
            }

            // Apply update
            survey.Title = item.Title;
            survey.Published = item.Published;

            await _surveyStore.UpdateSurveyAsync(survey);
            return new ObjectResult(DataMapping._surveyToDto(survey));
        }

        /// <summary>
        /// This method deletes the Survey with the specified id value.
        /// </summary>
        /// <param name="id">The id of the Survey</param>
        /// <returns>An ActionResult that contains the deleted Survey if deletion is successful or a Not Found response if the Survey is not found</returns>
        [HttpDelete("surveys/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var survey = await _surveyStore.GetSurveyAsync(id);
            if (survey == null)
            {
                return HttpNotFound();
            }

            // Validate that the current user has Delete permissions to this survey.
            if (!await _authorizationService.AuthorizeAsync(User, survey, Operations.Delete))
            {
                return new HttpStatusCodeResult(403);
            }

            await _surveyStore.DeleteSurveyAsync(survey);
            return new ObjectResult(DataMapping._surveyToDto(survey));
        }

        /// <summary>
        /// This method persists a survey contributor request.
        /// </summary>
        /// <param name="item">A ContributorRequest instance</param>
        /// <returns>A No Content response if the ContributorRequest is successful persisted 
        /// or a Bad Request response if the corresponding Survey is not found, 
        /// or the ContributorRequest is not valid</returns>
        [HttpPost("/surveys/{id}/contributorrequests")]
        public async Task<IActionResult> AddContributorRequest([FromBody] ContributorRequest item)
        {
            if (item == null)
            {
                return HttpBadRequest();
            }
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            var survey = await _surveyStore.GetSurveyAsync(item.SurveyId);
            if (survey == null)
            {
                return HttpBadRequest();
            }

            // Validate that the current user has Update permissions to this survey.
            if (!await _authorizationService.AuthorizeAsync(User, survey, Operations.Update))
            {
                return new HttpStatusCodeResult(403);
            }

            await _contributorRequestStore.AddRequestAsync(item);

            return new NoContentResult();
        }

        /// <summary>
        /// This method processes pending ContributorRequests for the current user.
        /// </summary>
        /// <returns>A No Content response</returns>
        [HttpPost("surveys/processpendingcontributorrequests")]
        public async Task<IActionResult> ProcessPendingContributorRequests()
        {
            var emailAddress = User.GetEmailValue();

            // Get pending ContributorRequests that match user's email address
            var contributorRequests = await _contributorRequestStore.GetRequestsForUserAsync(emailAddress);

            foreach (var contributorRequest in contributorRequests)
            {
                var survey = await _surveyStore.GetSurveyAsync(contributorRequest.SurveyId);

                int contributorId = User.GetUserKey();

                // Check for existing contributor assignment
                if (!survey.Contributors.Any(x => x.UserId == contributorId))
                {
                    survey.Contributors.Add(new SurveyContributor { SurveyId = contributorRequest.SurveyId, UserId = contributorId });
                    await _surveyStore.UpdateSurveyAsync(survey);
                }

                await _contributorRequestStore.RemoveRequestAsync(contributorRequest);
            }

            return new NoContentResult();
        }

        /// <summary>
        /// This method publishes the Survey with the matching id.
        /// </summary>
        /// <param name="id">The id of the Survey</param>
        /// <returns>An ActionResult that contains a SurveyDTO of a published Survey, or a Not Found response if the survey is not found</returns>
        [HttpPut("surveys/{id}/publish")]
        public async Task<IActionResult> Publish(int id)
        {
            var survey = await _surveyStore.GetSurveyAsync(id);
            if (survey == null)
            {
                return HttpNotFound();
            }

            // Validate that the current user has Publish permissions to this survey.
            if (!await _authorizationService.AuthorizeAsync(User, survey, Operations.Publish))
            {
                return new HttpStatusCodeResult(403);
            }

            var published = await _surveyStore.PublishSurveyAsync(id);

            return new ObjectResult(DataMapping._surveyToDto(published));
        }

        /// <summary>
        /// This method unpublishes the Survey with the matching id.
        /// </summary>
        /// <param name="id">The id of the Survey</param>
        /// <returns>An ActionResult that contains a SurveyDTO of an unpublished Survey, or a Not Found response if the survey is not found</returns>
        [HttpPut("surveys/{id}/unpublish")]
        public async Task<IActionResult> UnPublish(int id)
        {
            var survey = await _surveyStore.GetSurveyAsync(id);
            if (survey == null)
            {
                return HttpNotFound();
            }

            // Validate that the current user has UnPublish permissions to this survey.
            if (!await _authorizationService.AuthorizeAsync(User, survey, Operations.UnPublish))
            {
                return new HttpStatusCodeResult(403);
            }

            var unpublished = await _surveyStore.UnPublishSurveyAsync(id);

            return new ObjectResult(DataMapping._surveyToDto(unpublished));
        }

    }
}
