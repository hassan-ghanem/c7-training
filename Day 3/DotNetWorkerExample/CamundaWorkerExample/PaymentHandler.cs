using Camunda.Worker;
using Camunda.Worker.Variables;
using Microsoft.Extensions.Logging;

namespace CamundaWorkerExample;

[HandlerTopics("process-payment", LockDuration = 60000)]
public class PaymentHandler : IExternalTaskHandler
{
    private readonly ILogger<PaymentHandler> _logger;

    public PaymentHandler(ILogger<PaymentHandler> logger)
    {
        _logger = logger;
    }

    public async Task<IExecutionResult> HandleAsync(ExternalTask externalTask, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing payment for task {TaskId}", externalTask.Id);

        try
        {
            // Get variables from the external task
            var amount = 0.0;
            var customerId = "";

            if (externalTask.TryGetVariable<DoubleVariable>("amount", out var amountVar))
            {
                amount = amountVar.Value;
            }

            if (externalTask.TryGetVariable<StringVariable>("customerId", out var customerIdVar))
            {
                customerId = customerIdVar.Value;
            }

            // Print out all variables
            _logger.LogInformation("=== Payment Task Variables ===");
            _logger.LogInformation("Task ID: {TaskId}", externalTask.Id);
            _logger.LogInformation("Amount: ${Amount}", amount);
            _logger.LogInformation("Customer ID: {CustomerId}", customerId);
            _logger.LogInformation("Retries: {Retries}", externalTask.Retries);
            _logger.LogInformation("===============================");


            // Simulate processing delay like the sample
            await Task.Delay(1000, cancellationToken);

            // Commented out throw exception statement for demo sending failure
            //throw new Exception("Payment service is temporarily unavailable");

            // // Always return success with payment status
            // _logger.LogInformation("Payment processed successfully for task {TaskId}", externalTask.Id);

            //// Return success with output variables
            //return new CompleteResult
            //{
            //    Variables = new Dictionary<string, VariableBase>
            //    {
            //        ["paymentStatus"] = new StringVariable("completed")
            //    }
            //};

            // Commented out failure case
            _logger.LogWarning("Payment failed for task {TaskId}", externalTask.Id);
            return new BpmnErrorResult("PAYMENT_FAILED", "Payment processing failed");
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment for task {TaskId}", externalTask.Id);

            // Calculate remaining retries following Java pattern
            int remainingRetries = externalTask.Retries != null ? externalTask.Retries.Value - 1 : 2;
            int retryInterval = 5000;

            _logger.LogInformation("Exception occurred. Current retries: {CurrentRetries}, Remaining retries: {RemainingRetries}",
                externalTask.Retries, remainingRetries);

            if (remainingRetries > 0)
            {
                _logger.LogWarning("Retrying task {TaskId}. Remaining retries: {RemainingRetries}",
                    externalTask.Id, remainingRetries);

                // Return failure with calculated remaining retries
                return new FailureResult("Payment processing error - will retry", ex.Message)
                {
                    Retries = remainingRetries,
                    RetryTimeout = retryInterval
                };
            }
            else
            {
                _logger.LogError("No retries remaining for task {TaskId}. Camunda will create an incident.",
                    externalTask.Id);

                // Final failure with 0 retries
                return new FailureResult("Payment processing error - no retries left", ex.Message)
                {
                    Retries = 0,
                    RetryTimeout = 0
                };
            }
        }
    }


}
