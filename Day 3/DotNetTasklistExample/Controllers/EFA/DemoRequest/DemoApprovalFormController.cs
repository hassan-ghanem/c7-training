using DemoTasklist.Models;
using DemoTasklist.Models.EFA.DemoRequest;
using DemoTasklist.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DemoTasklist.Controllers.EFA.DemoRequest
{
    public class DemoApprovalFormController : Controller
    {
        private readonly CamundaService _camundaService;

        public DemoApprovalFormController(CamundaService camundaService)
        {
            _camundaService = camundaService;
        }

        public async Task<IActionResult> Index(string taskId, string processInstanceId)
        {
            try
            {
                var variables = await _camundaService.GetTaskVariables(taskId);
                var demoRequest = variables["demoRequest"].GetValue<DemoRequestModel>();

                var approvalFormModel = new DemoApprovalFormModel
                {
                    TaskId = taskId,
                    ProcessInstanceId = processInstanceId,
                    Name = demoRequest.Name,
                    Description = demoRequest.Description
                };

                return View("Views/EFA/Forms/DemoRequest/DemoApprovalForm.cshtml", approvalFormModel);
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
                    ViewBag.ErrorMessage = "An unexpected error occurred while loading the task details.";
                }

                return View("ErrorPage");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitDemoApprovalForm(DemoApprovalFormModel model)
        {
            try
            {
                var variables = await _camundaService.GetTaskVariables(model.TaskId);
                var demoRequest = variables["demoRequest"].GetValue<DemoRequestModel>();

                demoRequest.Approver_Comments = model.ApproverComments;
                demoRequest.Approver_Action_DT = DateTime.Now;
                demoRequest.Approver_Response = model.ApproverAction == "Approved"
                    ? "approved"
                    : model.ApproverAction;

                var newVariables = new Dictionary<string, object>
                {
                    { "demoRequest", demoRequest },
                    { "approverDecision", model.ApproverAction }
                };

                await _camundaService.CompleteTask(
                    model.TaskId,
                    model.ApproverAction,
                    model.ApproverComments,
                    newVariables);

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
                    ViewBag.ErrorMessage = "An unexpected error occurred while submitting the task.";
                }

                return View("ErrorPage");
            }
        }
    }
}
