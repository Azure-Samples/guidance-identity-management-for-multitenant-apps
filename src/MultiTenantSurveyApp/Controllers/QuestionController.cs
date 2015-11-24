// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using MultiTenantSurveyApp.DAL.DTOs;
using MultiTenantSurveyApp.Security;
using MultiTenantSurveyApp.Services;

namespace MultiTenantSurveyApp.Controllers
{
    /// <summary>
    /// This MVC controller provides actions for the management of questions.
    /// The actions in this controller class require the user to be signed in.
    /// </summary>
    [Authorize]
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;
        private readonly SignInManager _signInManager;

        public QuestionController(IQuestionService questionsClient, SignInManager signInManager)
        {
            _questionService = questionsClient;
            _signInManager = signInManager;
        }

        /// <summary>
        /// This action provides the Http Get experience for creating a question in the context of a survey.
        /// </summary>
        /// <param name="id">The id of a survey</param>
        /// <returns>A view with form fields for a new question</returns>
        public IActionResult Create(int id)
        {
            var question = new QuestionDTO { SurveyId = id };
            return View(question);
        }

        /// <summary>
        /// This action provides the Http Post experience for creating a question.
        /// </summary>
        /// <param name="question">The QuestionDTO instance that contains the fields necessary to create a question</param>
        /// <returns>A view that either shows validation errors or a redirection to the Survey Edit experience</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionDTO question)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _questionService.CreateQuestionAsync(question);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Edit", "Survey", new { id = question.SurveyId });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, $"Unable to create question. (HTTP {result.StatusCode})");
                        switch (result.StatusCode)
                        {
                            case (int)HttpStatusCode.Unauthorized:
                                return await SignOut();
                            case (int)HttpStatusCode.Forbidden:
                                ViewBag.Forbidden = true;
                                return View(question);
                            default:
                                return View(question);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Bad Request");
                    return View(question);
                }
            }
            catch
            {
                // Errors have been logged by QuestionService. Swallowing exception to stay on same page to display error.
                ViewBag.Message = "Unexpected Error";
            }

            return View("~/Views/Shared/Error.cshtml");
        }

        /// <summary>
        /// This action provides the Http Get experience for editing a question.
        /// </summary>
        /// <param name="id">The id of a question</param>
        /// <returns>A view with form fields for the question being edited</returns>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var result = await _questionService.GetQuestionAsync(id);
                if (result.Succeeded)
                {
                    return View(result.Item);
                }

                if (result.StatusCode == (int)HttpStatusCode.Unauthorized)
                {
                    return await SignOut();
                }

                ViewBag.Message = result.StatusCode;
            }
            catch
            {
                ViewBag.Message = "Unexpected Error";
            }
            return View("~/Views/Shared/Error.cshtml");
        }

        /// <summary>
        /// This action provides the Http Post experience for editing a question.
        /// </summary>
        /// <param name="question">The QuestionDTO instance that contains the question's updated fields</param>
        /// <returns>A view that either shows validation errors or a redirection to the Survey Edit experience</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuestionDTO question)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _questionService.UpdateQuestionAsync(question);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Edit", "Survey", new { id = question.SurveyId });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, $"Unable to edit question. (HTTP {result.StatusCode})");
                        switch (result.StatusCode)
                        {
                            case (int)HttpStatusCode.Unauthorized:
                                return await SignOut();
                            case (int)HttpStatusCode.Forbidden:
                                ViewBag.Forbidden = true;
                                return View(question);
                            default:
                                return View(question);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Bad Request");
                    return View(question);
                }
            }
            catch
            {
                // Errors have been logged by QuestionService. Swallowing exception to stay on same page to display error.
                ViewBag.Message = "Unexpected Error";
            }

            return View("~/Views/Shared/Error.cshtml");
        }

        /// <summary>
        /// This action provides the Http Get experience for deleting a question.
        /// </summary>
        /// <param name="id">The id of the question</param>
        /// <returns>A view that shows a delete confirmation prompt</returns>
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _questionService.GetQuestionAsync(id);
                if (result.Succeeded)
                {
                    return View("Delete", result.Item);
                }

                if (result.StatusCode == (int)HttpStatusCode.Unauthorized)
                {
                    return await SignOut();
                }

                ViewBag.Message = result.StatusCode;
            }
            catch
            {
                ViewBag.Message = "Unexpected Error";
            }
            return View("~/Views/Shared/Error.cshtml");
        }

        /// <summary>
        /// This action provides the Http Post experience for deleting a question.
        /// </summary>
        /// <param name="question">The QuestionDTO instance that contains the id of the question to be deleted</param>
        /// <returns>A view that either shows validation errors or a redirection to the Survey Edit experience</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(QuestionDTO question)
        {
            try
            {
                var result = await _questionService.DeleteQuestionAsync(question.Id);
                if (result.Succeeded)
                {
                    return RedirectToAction("Edit", "Survey", new { id = question.SurveyId });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Unable to delete question. (HTTP {result.StatusCode})");
                    switch (result.StatusCode)
                    {
                        case (int)HttpStatusCode.Unauthorized:
                            return await SignOut();
                        case (int)HttpStatusCode.Forbidden:
                            ViewBag.Forbidden = true;
                            return View(question);
                        default:
                            return View(question);
                    }
                }
            }
            catch
            {
                // Errors have been logged by QuestionService. Swallowing exception to stay on same page to display error.
                ViewBag.Message = "Unexpected Error";
            }
            return View("~/Views/Shared/Error.cshtml");
        }

        private async Task<IActionResult> SignOut()
        {
            return await _signInManager.SignOutAsync(
                Url.Action("SignOutCallback", "Account", values: null, protocol: Request.Scheme));
        }
    }
}
