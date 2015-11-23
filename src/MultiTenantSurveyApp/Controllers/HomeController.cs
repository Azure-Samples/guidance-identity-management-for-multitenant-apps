// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Mvc;
using MultiTenantSurveyApp.Services;

namespace MultiTenantSurveyApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISurveyService _surveyService;

        public HomeController(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _surveyService.GetPublishedSurveysAsync();
            if (result.Succeeded)
            {
                return View(result.Item);
            }

            return new HttpStatusCodeResult(result.StatusCode);
        }

        public IActionResult Details(int id)
        {
            return View();
        }

        public IActionResult Error()
        {
            var error = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (error != null)
            {
                ViewBag.Message = error.Error.Message;
            }
            return View("~/Views/Shared/Error.cshtml");
        }

        public IActionResult Forbidden()
        {
            return View("~/Views/Shared/Forbidden.cshtml");
        }

        public IActionResult NotSignedUp()
        {
            return View();
        }
    }
}
