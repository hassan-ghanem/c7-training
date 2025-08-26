package org.example;

import org.springframework.stereotype.Component;
import java.util.HashMap;

import io.camunda.zeebe.spring.client.annotation.JobWorker;
import io.camunda.zeebe.spring.client.annotation.Variable;
import io.camunda.zeebe.spring.common.exception.ZeebeBpmnError;

@Component
public class ProcessPaymentJobWorker {

	@JobWorker(type = "process-payment", timeout = 30000)
	public void handleProcessPaymentJob(@Variable(name = "paymentDetails") HashMap<String, Object> paymentDetails) {

		try {

			System.out.println("Payment Details Received:");

			String customerId = (String) paymentDetails.get("customerId");
			Double amount = (Double) paymentDetails.get("amount");

			boolean successful = true;
			if (successful) {
			
				System.out.println("Customer ID: " + customerId);
				System.out.println("Amount: " + amount.toString());

			} else {

				throw new ZeebeBpmnError("PAYMENT_FAILED", "Payment failed", null);
			}
			

		} catch (Exception e) {

			System.err.println("Error while processing payment: " + e.getMessage());

			throw e;
		}
	}
}
