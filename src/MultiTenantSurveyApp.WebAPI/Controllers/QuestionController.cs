// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
    /// This class provides a REST based API for the management of questions.
    /// This class uses Bearer token authentication and authorization.
    /// </summary>
    [Authorize(ActiveAuthenticationSchemes = "Bearer")]
    public class QuestionController : Controller
    {
        private readonly IQuestionStore _questionStore;
        private readonly ISurveyStore _surveyStore;
        private readonly IAuthorizationService _authorizationService;

        public QuestionController(IQuestionStore questionStore, ISurveyStore surveyStore, IAuthorizationService authorizationService)
        {
            _surveyStore = surveyStore;
            _authorizationService = authorizationService;
            _questionStore = questionStore;
        }

        /// <summary>
        /// This method returns the Question with a matching id property.
        /// </summary>
        /// <param name="id">The id of the Question</param>
        /// <returns>An ActionResult that contains a QuestionDTO if found, otherwise a Not Found response</returns>
        [HttpGet("questions/{id:int}", Name = "GetQuestion")]
        public async Task<IActionResult> Get(int id)
        {
            var question = await _questionStore.GetQuestionAsync(id);
            if (question == null)
            {
                //Logger.LogInformation("Details: Item not found {0}", id);
                return HttpNotFound();
            }
            return new ObjectResult(DataMapping._questionToDto(question));
        }

        /// <summary>
        /// This method persists a new Question with the specified id value.
        /// </summary>
        /// <param name="id">The id of the question</param>
        /// <param name="questionDto">A DTO containing property values of the Question</param>
        /// <returns>A CreatedAtRouteResult of the newly created Question if successfully persisted</returns>
        [HttpPost("surveys/{id:int}/questions")]
        public async Task<IActionResult> Create(int id, [FromBody] QuestionDTO questionDto)
        {
            if (questionDto == null)
            {
                return HttpBadRequest();
            }
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            var question = new Question
            {
                SurveyId = id,
                Text = questionDto.Text,
                Type = questionDto.Type,
                PossibleAnswers = questionDto.PossibleAnswers
            };

            var survey = await _surveyStore.GetSurveyAsync(question.SurveyId);
            if (survey == null)
            {
                return HttpNotFound();
            }

            // The AuthorizationService uses the policies in the MultiTenantSurveyApp.Security project
            if (!await _authorizationService.AuthorizeAsync(User, survey, Operations.Update))
            {
                return new HttpStatusCodeResult(403);
            }


            await _questionStore.AddQuestionAsync(question);
            return CreatedAtRoute("GetQuestion", new { controller = "Question", id = question.Id }, questionDto);
        }

        /// <summary>
        /// This method updates the Question with the specified id value.
        /// </summary>
        /// <param name="id">The id of the Question</param>
        /// <param name="questionDto">A QuestionDTO containing property values of the Question</param>
        /// <returns>An ActionResult that contains a QuestionDTO containing property values of the updated Question</returns>
        [HttpPut("questions/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] QuestionDTO questionDto)
        {
            if (questionDto == null || questionDto.Id != id)
            {
                return HttpBadRequest();
            }
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            var question = await _questionStore.GetQuestionAsync(id);
            if (question == null)
            {
                return HttpNotFound();
            }

            var survey = await _surveyStore.GetSurveyAsync(question.SurveyId);
            if (survey == null)
            {
                return HttpNotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, survey, Operations.Update))
            {
                return new HttpStatusCodeResult(403);
            }


            // Apply update
            question.Text = questionDto.Text;
            question.PossibleAnswers = questionDto.PossibleAnswers;
            question.Type = questionDto.Type;
            question.PossibleAnswers = questionDto.PossibleAnswers;
            question.SurveyId = questionDto.SurveyId;

            var result = await _questionStore.UpdateQuestionAsync(question);

            return new ObjectResult(DataMapping._questionToDto(result));
        }

        /// <summary>
        /// This method deletes the Question with the specified id value.
        /// </summary>
        /// <param name="id">The id of the Question</param>
        /// <returns>A No Content response if deletion is successful or a Not Found response if the Question is not found</returns>
        [HttpDelete("questions/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _questionStore.GetQuestionAsync(id);
            if (question == null)
            {
                return HttpNotFound();
            }

            var survey = await _surveyStore.GetSurveyAsync(question.SurveyId);
            if (survey == null)
            {
                return HttpNotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, survey, Operations.Update))
            {
                return new HttpStatusCodeResult(403);
            }

            await _questionStore.DeleteQuestionAsync(question);
            return new NoContentResult();
        }
    }
}
