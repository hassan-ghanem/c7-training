namespace DemoTasklist.Helpers
{
    public static class ActionDisplayMapper
    {
        // Dictionary to store mappings
        private static readonly Dictionary<string, string> ActionDisplayMap = new()
        {
            { "auto-approved", "Auto Approved" },
            { "approved", "Approved" },
            { "rejected", "Rejected" },
            { "returned", "Returned for Ammendment" },
            { "done", "Done" },
            { "submitted", "Submitted" },
            { "transferred", "Transferred" },
            { "incomplete", "Incomplete"},
            { "results-sent", "Results Sent" },
            { "forwarded", "Forwarded" },
            { "updated", "Updated" }
        };

        // Method to retrieve display value
        public static string GetDisplayValue(string programmaticValue)
        {
            return ActionDisplayMap.TryGetValue(programmaticValue, out var displayValue)
                ? displayValue
                : programmaticValue; 
        }
    }
}
