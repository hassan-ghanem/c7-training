using System.Text.Json;

namespace DemoTasklist.Helpers
{
    public static class JsonHelper
    {
        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json)) return default;

            try
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
                return default; // Returns null for reference types, 0/false for value types
            }
        }
    }
}
