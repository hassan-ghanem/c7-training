namespace DemoTasklist.Models.EFA.DemoRequest
{
    public class DemoRequestModel
    {
        // Demo specific fields - only what user provides
        public string? Name { get; set; }
        public string? Description { get; set; }

        // Approver fields for workflow
        public string? Approver_Response { get; set; }
        public DateTime Approver_Action_DT { get; set; }
        public string? Approver_Comments { get; set; }
    }
}
