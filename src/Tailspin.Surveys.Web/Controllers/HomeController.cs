// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Tailspin.Surveys.Web.Services;

namespace Tailspin.Surveys.Web.Controllers
{
    /// <summary>
    /// This controller provides MVC actions for the Home, Error and Forbidden experiences.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ISurveyService _surveyService;

        public HomeController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        /// <summary>
        /// This action provides the Home experience.
        /// </summary>
        /// <returns>A view that shows all the published <see cref="Tailspin.Surveys.Data.DataModels.Survey"/>s.</returns>
        public async Task<IActionResult> Index()
        {
            var result = await _surveyService.GetPublishedSurveysAsync();
            if (result.Succeeded)
            {
                return View(result.Item);
            }

            return StatusCode(result.StatusCode);
        }

        /// <summary>
        /// This action provides a placeholder for the experience
        /// of taking a <see cref="Tailspin.Surveys.Data.DataModels.Survey"/>.
        /// </summary>
        /// <param name="id">The id of a <see cref="Tailspin.Surveys.Data.DataModels.Survey"/></param>
        /// <returns>A view that shows a placeholder for the experience of taking a <see cref="Tailspin.Surveys.Data.DataModels.Survey"/></returns>
        public IActionResult Details(int id)
        {
            return View();
        }

        /// <summary>
        /// This action provides a general error experience.
        /// </summary>
        /// <returns>A view that shows an error message if available</returns>
        public IActionResult Error()
        {
            var error = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (error != null)
            {
                ViewBag.Message = error.Error.Message;
            }
            return View("~/Views/Shared/Error.cshtml");
        }

        /// <summary>
        /// This action provides a forbidden access experience.
        /// </summary>
        /// <returns>A view that tells the user that they are not allowed to access the requested resource</returns>
        public IActionResult Forbidden()
        {
            return View("~/Views/Shared/Forbidden.cshtml");
        }
    }
}
