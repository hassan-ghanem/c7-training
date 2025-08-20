using Camunda.Api.Client.UserTask;
using Microsoft.AspNetCore.Mvc;
using DemoTasklist.Services;
using DemoTasklist.Models;
using System.Text.Json;
using Camunda.Api.Client;

namespace DemoTasklist.Controllers
{
    public class TaskListController : Controller
    {
        private readonly CamundaService _camundaService;
        private const int PageSize = 3;

        public TaskListController(CamundaService camundaService)
        {
            _camundaService = camundaService;
        }

        public async Task<IActionResult> Index(string sortOrder, int pageNumber = 1)
        {
            try
            {
                // Set default sorting to Created descending if no sortOrder is provided
                sortOrder ??= "created_desc";

                // Define sorting parameters
                ViewBag.NameSortParam = sortOrder == "name" ? "name_desc" : "name";
                ViewBag.AssigneeSortParam = sortOrder == "assignee" ? "assignee_desc" : "assignee";
                ViewBag.CreatedSortParam = sortOrder == "created" ? "created_desc" : "created";
                ViewBag.CurrentSort = sortOrder;

                // Create a mapping for sort order to TaskSorting and SortOrder
                var sortingOptions = new Dictionary<string, (TaskSorting, SortOrder)>
                {
                    { "name", (TaskSorting.Name, SortOrder.Ascending) },
                    { "name_desc", (TaskSorting.Name, SortOrder.Descending) },
                    { "assignee", (TaskSorting.Assignee, SortOrder.Ascending) },
                    { "assignee_desc", (TaskSorting.Assignee, SortOrder.Descending) },
                    { "created", (TaskSorting.Created, SortOrder.Ascending) },
                    { "created_desc", (TaskSorting.Created, SortOrder.Descending) }
                };

                // Determine the sorting option
                var (taskSorting, sortOrderDirection) = sortingOptions.TryGetValue(sortOrder, out var option)
                    ? option
                    : (TaskSorting.Created, SortOrder.Descending);

                // Calculate the first result and page size
                var firstResult = (pageNumber - 1) * PageSize;

                // Query tasks with pagination and sorting
                var userTaskQuery = new TaskQuery().Sort(taskSorting, sortOrderDirection);
                var tasks = await _camundaService.GetTasks(userTaskQuery, firstResult, PageSize);

                // Get the total count of tasks to calculate total pages
                var totalTasks = await _camundaService.GetTasksCount(userTaskQuery);
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalTasks / PageSize);
                ViewBag.CurrentPage = pageNumber;

                return View(tasks); // Points to Views/TaskList/Index.cshtml

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
    }
}