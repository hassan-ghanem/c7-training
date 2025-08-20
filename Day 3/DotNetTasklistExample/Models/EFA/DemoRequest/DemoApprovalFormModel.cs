using System.ComponentModel.DataAnnotations;

namespace DemoTasklist.Models.EFA.DemoRequest
{
    public class DemoApprovalFormModel : DemoRequestViewModel
    {
        public string? TaskId { get; set; }
        public string? ProcessInstanceId { get; set; }
        [Required(ErrorMessage = "Please select an action.")]
        public string? ApproverAction { get; set; }
        public string? ApproverComments { get; set; }
    }
}
