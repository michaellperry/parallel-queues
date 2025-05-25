# WiredBrain Microservices Demo: Resiliency Patterns

This repository contains a demo application that showcases various resiliency patterns in microservices architecture. The demo is built using .NET 9, MassTransit, and Poly. To run the demo, use the Docker Compose file in the `mesh` directory.

```bash
cd mesh
docker compose pull
docker compose build
docker compose up -d
```

## The Scenario

The billing service calls a third-party payment service (simulated by the `Simulated.Payments` project). The demo shows the total amount of money billed in the billing service, and the total amount of money paid in the payment service. If the system is able to recover from failures, these totals should match.

When the system is under load, the payment service may take a long time. This will trigger a timeout and a retry in the billing service. The payment service is providing consistency guarantees, so it will ignore the duplicates that are thus created. However, the extra load of these retries and idempotency checks will delay the recovery of the payment service.

To resolve this problem, the billing service will use a circuit breaker pattern to stop sending requests to the payment service for a while. This will allow the payment service to recover more quickly and then accept new requests.

The circuit breaker will solve the downstream problem, but it will also cause messages to be sent to the error queue. To resolve this issue, we will switch from using a circuit breaker to using a kill switch.

## Running the Demo

Once the Docker Compose stack is running, you will see logs from the billing service indicating that it is processing orders. Run the following command to see the logs:

```bash
docker compose logs -f billing
```

Similarly, you can view the logs for the payment service:

```bash
docker compose logs -f simulated-payments
```

Access the billing service's API at `http://localhost:8080/swagger`. Use this to request the current total amount charged. Then access the simulated payment service's API at `http://localhost:8082/swagger` to see the total amount paid. While the system is running, these two totals will be updated in real-time. Stop the ordering service in order to pause the system and let it come to rest. Then you can verify that the totals match.

### Thrashing Under Load

Start the ordering service again to resume the traffic. Now you want to put the simulated payment service into a mode where it will take a long time to respond. Use the API to PUT a delay value of 10 seconds. Observe the logs to see that the billing service is waiting on the payment service. After 5 seconds, it will time out and retry. Meanwhile, observe the logs of the simulated payment service to see that it eventually completes the payment. Also observe that it recognizes the duplicate payment request on retry and ignores it.

After a while, you will see that most of the traffic to the payment service is duplicate transactions. This extra load will prevent the payment service from recovering.

### Implementing the Circuit Breaker

To resolve this, change the billing service to use the circuit breaker pattern. Uncomment the `RegisterCircuitBreakerPolicy` line in the `ResiliencePolicyRegistry` of the billing service and rebuild the Docker images:

```bash
docker compose up --build -d
```

This restarts the billing service. It persists the total amount charged, so you will be able to see the system recover. Observe the logs of the billing service and notice that it stops sending requests to the payment service for a while. Then it resumes sending requests in half-open mode, only to notice that the payment service is still overloaded. Then it stops for a while longer.

Use the RabbitMQ management console to see the error queue. You will see that the billing service is sending messages to the error queue. This is because the circuit breaker is throwing an exception in the consumer. The message is Nacked and sent to the error queue.

Use the API of the simulated payment service to set the delay back to 0 seconds. This will allow the payment service to recover. After a while, you will see that the billing service resumes sending requests to the payment service. It gets caught up with the backlog of requests. Stop the ordering service again to let the system come to rest. The totals will not match. Some of the requests were sent to the error queue and never processed.

Shovel the error queue back into the main queue. This will reprocess the requests that were sent to the error queue. You can do this using the RabbitMQ management console.

### Implementing the Kill Switch

To resolve the problem of messages being sent to the error queue, switch from using a circuit breaker to using a kill switch. Uncomment the `UseKillSwitch` line in the `Program.cs` file of the billing service and rebuild the Docker images:

```bash
docker compose up --build -d
```

Again change the delay of the payment service to 10 seconds. Observe the logs of the billing service. You will see that it stops sending requests to the payment service for a while. Then it resumes sending requests after a while. The difference is that while it waits for the payment service to recover, it does not draw any messages from the queue. This means that the RabbitMQ queue holds on to the messages until the billing service is ready to process them again. They will not be sent to the error queue.

Return the delay of the payment service to 0 seconds. The billing service will start processing messages again. You will see that it is able to process all the messages in the queue without sending any to the error queue. The system will recover more quickly. Stop the ordering service again to let the system come to rest. The totals will match this time.
