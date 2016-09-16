// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailspin.Surveys.Data.DTOs;
using Tailspin.Surveys.Web.Security;
using Tailspin.Surveys.Web.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Tailspin.Surveys.Web.Models;
using Microsoft.AspNetCore.Http.Authentication;

namespace Tailspin.Surveys.Web.Controllers
{
    /// <summary>
    /// This MVC controller provides actions for the management of <see cref="Tailspin.Surveys.Data.DataModels.Question"/>s.
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
        /// This action provides the Http Get experience for creating a <see cref="Tailspin.Surveys.Data.DataModels.Question"/> in the context of a <see cref="Tailspin.Surveys.Data.DataModels.Survey"/>.
        /// </summary>
        /// <param name="id">The id of a <see cref="Tailspin.Surveys.Data.DataModels.Survey"/></param>
        /// <returns>A view with form fields for a new <see cref="Tailspin.Surveys.Data.DataModels.Question"/></returns>
        public IActionResult Create(int id)
        {
            var question = new QuestionDTO { SurveyId = id };
            return View(question);
        }

        /// <summary>
        /// This action provides the Http Post experience for creating a <see cref="Tailspin.Surveys.Data.DataModels.Question"/>.
        /// </summary>
        /// <param name="question">The <see cref="QuestionDTO"/> instance that contains the fields necessary to create a <see cref="Tailspin.Surveys.Data.DataModels.Question"/></param>
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
                                return ReAuthenticateUser();
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
            catch (AuthenticationException)
            {
                return ReAuthenticateUser();
            }
            catch
            {
                // Errors have been logged by QuestionService. Swallowing exception to stay on same page to display error.
                ViewBag.Message = "Unexpected Error";
            }

            return View("~/Views/Shared/Error.cshtml");
        }

        /// <summary>
        /// This action provides the Http Get experience for editing a <see cref="Tailspin.Surveys.Data.DataModels.Question"/>.
        /// </summary>
        /// <param name="id">The id of a <see cref="Tailspin.Surveys.Data.DataModels.Question"/></param>
        /// <returns>A view with form fields for the <see cref="Tailspin.Surveys.Data.DataModels.Question"/> being edited</returns>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var result = await _questionService.GetQuestionAsync(id);
                if (result.Succeeded)
                {
                    return View(result.Item);
                }

                var errorResult = CheckStatusCode(result);
                if (errorResult != null) return errorResult;

                ViewBag.Message = "Unexpected Error";
            }
            catch (AuthenticationException)
            {
                return ReAuthenticateUser();
            }
            catch
            {
                ViewBag.Message = "Unexpected Error";
            }
            return View("~/Views/Shared/Error.cshtml");
        }

        /// <summary>
        /// This action provides the Http Post experience for editing a <see cref="Tailspin.Surveys.Data.DataModels.Question"/>.
        /// </summary>
        /// <param name="question">The <see cref="QuestionDTO"/> instance that contains the <see cref="Tailspin.Surveys.Data.DataModels.Question"/>'s updated fields</param>
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
                                return ReAuthenticateUser();
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
            catch (AuthenticationException)
            {
                return ReAuthenticateUser();
            }
            catch
            {
                // Errors have been logged by QuestionService. Swallowing exception to stay on same page to display error.
                ViewBag.Message = "Unexpected Error";
            }

            return View("~/Views/Shared/Error.cshtml");
        }

        /// <summary>
        /// This action provides the Http Get experience for deleting a <see cref="Tailspin.Surveys.Data.DataModels.Question"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Tailspin.Surveys.Data.DataModels.Question"/></param>
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

                var errorResult = CheckStatusCode(result);
                if (errorResult != null) return errorResult;

                ViewBag.Message = "Unexpected Error";
            }
            catch (AuthenticationException)
            {
                return ReAuthenticateUser();
            }
            catch
            {
                ViewBag.Message = "Unexpected Error";
            }
            return View("~/Views/Shared/Error.cshtml");
        }

        /// <summary>
        /// This action provides the Http Post experience for deleting a <see cref="Tailspin.Surveys.Data.DataModels.Question"/>.
        /// </summary>
        /// <param name="question">The <see cref="QuestionDTO"/> instance that contains the id of the <see cref="Tailspin.Surveys.Data.DataModels.Question"/> to be deleted</param>
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
                            return ReAuthenticateUser();
                        case (int)HttpStatusCode.Forbidden:
                            ViewBag.Forbidden = true;
                            return View(question);
                        default:
                            return View(question);
                    }
                }
            }
            catch (AuthenticationException)
            {
                return ReAuthenticateUser();
            }
            catch
            {
                // Errors have been logged by QuestionService. Swallowing exception to stay on same page to display error.
                ViewBag.Message = "Unexpected Error";
            }
            return View("~/Views/Shared/Error.cshtml");
        }

        private IActionResult CheckStatusCode(ApiResult result)
        {
            if (result.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                return ReAuthenticateUser();
            }

            if (result.StatusCode == (int)HttpStatusCode.Forbidden)
            {
                // Redirects user to Forbidden page
                return new ChallengeResult(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            if (result.StatusCode == (int)HttpStatusCode.NotFound)
            {
                ModelState.AddModelError(string.Empty, "The survey can not be found");
                ViewBag.Message = "The survey can not be found";
                return View("~/Views/Shared/Error.cshtml");
            }

            return null;
        }

        private IActionResult ReAuthenticateUser()
        {
            return new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    RedirectUri = Url.Action("SignInCallback", "Account")
                });
        }
    }
}
