using Microsoft.AspNetCore.Mvc;

namespace DemoTasklist.Controllers
{
    public class RedirectController : Controller
    {
        // Action that renders the RedirectForm.cshtml
        public IActionResult RedirectForm(string formKey, string taskId, string processInstanceId)
        {
            // Pass the query parameters to the view
            ViewBag.FormKey = formKey;
            ViewBag.TaskId = taskId;
            ViewBag.ProcessInstanceId = processInstanceId;

            return View();
        }
    }
}
