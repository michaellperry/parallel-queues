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

## Running the Demo

Once the Docker Compose stack is running, you will see logs from the billing service indicating that it is processing orders. Run the following command to see the logs:

```bash
docker compose logs -f billing
```

Similarly, you can view the logs for the payment service:

```bash
docker compose logs -f simulated-payments
```

Access the billing service's API at `http://localhost:8080/swagger`. Use this to request the current total amount charged. Then access the simulated payment service's API at `http://localhost:8081/swagger` to see the total amount paid. While the system is running, these two totals will be updated in real-time. Stop the ordering service in order to pause the system and let it come to rest. Then you can verify that the totals match.

Start the ordering service again to resume the traffic. Now you want to put the simulated payment service into a mode where it will take a long time to respond. Use the API to PUT a delay value of 10 seconds. Observe the logs to see that the billing service is waiting on the payment service. After 5 seconds, it will time out and retry. Meanwhile, observe the logs of the simulated payment service to see that it eventually completes the payment. Also observe that it recognizes the duplicate payment request on retry and ignores it.

After a while, you will see that most of the traffic to the payment service is duplicate transactions. This extra load will prevent the payment service from recovering. To resolve this, change the billing service to use the circuit breaker pattern. Uncomment the `UseCircuitBreaker` line in the `Program.cs` file of the billing service and rebuild the Docker images:

```bash
docker compose up --build -d
```

This restarts the billing service. It persists the total amount charged, so you will be able to see the system recover. Observe the logs of the billing service and notice that it stops sending requests to the payment service for a while. Then it resumes sending requests, only to notice that the payment service is still overloaded. Then it stops for a while longer.

Use the API of the simulated payment service to set the delay back to 0 seconds. This will allow the payment service to recover. After a while, you will see that the billing service resumes sending requests to the payment service. It gets caught up with the backlog of requests. Stop the ordering service again to let the system come to rest. Then verify that the totals match again.
