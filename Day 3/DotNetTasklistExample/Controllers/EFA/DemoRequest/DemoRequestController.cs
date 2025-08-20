using DemoTasklist.Models;
using DemoTasklist.Models.EFA.DemoRequest;
using DemoTasklist.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DemoTasklist.Controllers.EFA.DemoRequest
{
    public class DemoRequestController : Controller
    {
        private readonly CamundaService _camundaService;

        public DemoRequestController(CamundaService camundaService)
        {
            _camundaService = camundaService;
        }

        public IActionResult DemoRequest()
        {
            var model = new DemoRequestViewModel();

            return View("Views/EFA/Forms/DemoRequest/DemoRequest.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitDemoRequest(DemoRequestViewModel model)
        {
            try
            {
                var demoRequest = new DemoRequestModel
                {
                    Name = model.Name,
                    Description = model.Description
                };

                var variables = new Dictionary<string, object>
                {
                    { "demoRequest", demoRequest }
                };

                string? processInstanceId = await _camundaService.StartProcessInstance("demo_request", variables, null);

                return RedirectToAction("Index", "TaskList");
            }
            catch (Exception ex)
            {
                if (ex is Refit.ApiException apiException)
                {
                    string apiErrorMessage = "An unexpected API error occurred.";

                    if (!string.IsNullOrEmpty(apiException.Content) && apiException.Content.Contains("message"))
                    {
                        var errorObject = JsonSerializer.Deserialize<ApiErrorResponse>(apiException.Content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (!string.IsNullOrEmpty(errorObject?.Message))
                        {
                            apiErrorMessage = errorObject.Message;
                        }
                    }

                    ViewBag.ErrorTitle = "API Error";
                    ViewBag.ErrorMessage = apiErrorMessage;
                }
                else
                {
                    ViewBag.ErrorTitle = "Unexpected Error";
                    ViewBag.ErrorMessage = "An unexpected error occurred while starting the process.";
                }

                return View("ErrorPage");
            }
        }
    }
}
