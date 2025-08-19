using Camunda.Worker;
using Camunda.Worker.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CamundaWorkerExample;

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;
                var camundaConfig = configuration.GetSection("CamundaWorker");

                // Configure Camunda Worker
                var camundaRestApiUri = camundaConfig["BaseAddress"] ?? "http://localhost:8080/engine-rest";

                // Configure Camunda External Task Client
                services.AddExternalTaskClient(client =>
                {
                    client.BaseAddress = new Uri(camundaRestApiUri);
                });

                // Configure Camunda Worker
                services.AddCamundaWorker("dotnet-external-worker")
                    .AddHandler<PaymentHandler>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Starting Camunda External Worker...");
        logger.LogInformation("Camunda Engine URL: {BaseAddress}",
            host.Services.GetRequiredService<IConfiguration>()["CamundaWorker:BaseAddress"]);
        logger.LogInformation("Worker ID: dotnet-external-worker");
        logger.LogInformation("Registered handlers: PaymentHandler (process-payment)");
        logger.LogInformation("Press Ctrl+C to stop the worker");

        try
        {
            await host.RunAsync();
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Worker stopped gracefully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Worker stopped due to an error");
        }
    }
}
