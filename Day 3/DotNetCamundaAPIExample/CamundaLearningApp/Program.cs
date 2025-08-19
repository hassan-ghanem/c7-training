using Camunda.Api.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CamundaLearningApp;

class Program
{
    static async Task Main(string[] args)
    {
        // Create host builder with dependency injection
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Configure Camunda API client
                services.AddSingleton<CamundaClient>(provider =>
                {
                    var configuration = provider.GetRequiredService<IConfiguration>();
                    var camundaUrl = configuration["Camunda:BaseUrl"] ?? "http://localhost:8080/engine-rest";

                    // For basic authentication, you would need to configure HttpClient manually
                    // For this learning example, we'll use the simple Create method
                    return CamundaClient.Create(camundaUrl);
                });

                services.AddTransient<CamundaService>();
                services.AddTransient<SimpleCamundaService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        // Get the service and run examples
        var camundaService = host.Services.GetRequiredService<CamundaService>();

        try
        {
            await camundaService.RunExamplesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Make sure Camunda is running on http://localhost:8080");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
