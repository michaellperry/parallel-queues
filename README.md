# WiredBrain Microservices Demo: Little's Law and Queueing Theory

This project demonstrates a microservices architecture for the WiredBrain coffee shop, consisting of Ordering, Billing, and Shipping services that communicate via RabbitMQ. It has been enhanced to demonstrate Little's Law and fundamental queueing theory concepts using metrics, Prometheus, and Grafana.

## Project Structure

- **WiredBrain.Ordering**: Service that handles customer orders and provides configuration API
- **WiredBrain.Billing**: Service that processes payments
- **WiredBrain.Shipping**: Service that prepares shipments
- **WiredBrain.Messages**: Shared message contracts
- **mesh**: Infrastructure components (RabbitMQ, Prometheus, Grafana)

## Prerequisites

- Docker and Docker Compose
- .NET 9.0 SDK (for development)

## Key Concepts Demonstrated

### Little's Law (L = λW)

Little's Law is a fundamental principle in queueing theory that states:

**The average number of items in a system (L) equals the average arrival rate (λ) multiplied by the average time an item spends in the system (W).**

This project demonstrates this law by:
- Tracking the arrival rate of orders (λ)
- Measuring the time orders spend in each service (W)
- Observing the queue lengths (L)
- Verifying that L ≈ λW in various scenarios

### Queueing Theory Concepts

The demo also illustrates several important queueing theory concepts:

1. **Underload vs. Overload**: What happens when arrival rate is less than, equal to, or greater than service rate
2. **Impact of Service Time**: How changing processing time affects queue length and wait time
3. **Bottleneck Identification**: How the slowest service in a chain becomes the system bottleneck
4. **Service Rate (μ = 1/τ)**: The relationship between processing time and service capacity
5. **Time in Queue vs. Time in System**: The distinction between waiting time and total time

## Running the Demo

### Configuration API

The WiredBrain.Ordering service now provides a REST API for configuring the queueing behavior of the system. This allows you to experiment with different scenarios without restarting the services.

#### Using the Swagger UI

The easiest way to interact with the configuration API is through the Swagger UI:

1. Start the services as described below
2. Open a browser and navigate to `http://localhost:5000/swagger` (or the appropriate port if different)
3. You'll see the Swagger UI with the available endpoints:
   - `GET /api/Configuration` - Get the current configuration
   - `POST /api/Configuration` - Update the configuration manually
   - `POST /api/Configuration/scenario/{scenarioName}` - Apply a predefined scenario

#### Available Scenarios

You can quickly apply predefined scenarios by making a POST request to `/api/Configuration/scenario/{scenarioName}` where `scenarioName` is one of:

1. **underload**: Demonstrates a system under minimal load
   - OrderArrivalRateMs: 2000 (0.5 orders/sec)
   - BillingProcessingDelayMs: 200 (service rate = 5 orders/sec)
   - Result: Minimal queueing, W ≈ τ

2. **nearcapacity**: Demonstrates a system operating near its capacity
   - OrderArrivalRateMs: 350 (2.86 orders/sec)
   - BillingProcessingDelayMs: 200 (service rate = 5 orders/sec)
   - Result: Increasing queue lengths

3. **overload**: Demonstrates a system under excessive load
   - OrderArrivalRateMs: 250 (4 orders/sec)
   - BillingProcessingDelayMs: 200 (service rate = 5 orders/sec)
   - Result: Continuously growing queues

4. **servicetime**: Demonstrates the impact of service time on queue length
   - OrderArrivalRateMs: 300 (3.33 orders/sec)
   - BillingProcessingDelayMs: 200 (service rate = 5 orders/sec)
   - Result: Shows how changing τ affects queue length

5. **bottleneck**: Demonstrates bottleneck identification
   - OrderArrivalRateMs: 300 (3.33 orders/sec)
   - BillingProcessingDelayMs: 400 (service rate = 2.5 orders/sec)
   - Result: Shows how the slowest service becomes the bottleneck

#### Custom Configuration

You can also set custom values by making a POST request to `/api/Configuration` with a JSON body:

```json
{
  "orderArrivalRateMs": 500,
  "billingProcessingDelayMs": 300
}
```

### Setup

If you prefer to run the services manually:

#### 1. Start the Services

From the root directory of the project, run:

```bash
cd mesh
docker-compose up -d --build
```

This will:
- Start a RabbitMQ container
- Build and start the Ordering, Billing, and Shipping service containers
- Start Prometheus and Grafana containers for metrics collection and visualization

#### 2. Manually Start the Shipping Service

The Shipping service container is configured to start without automatically running the application. This allows you to connect to the container and manually start the service when needed.

To connect to the Shipping container:

```bash
# Get the container ID or name
docker ps

# Connect to the container (replace [container_id] with the actual ID or name)
docker exec -it [container_id] bash
```

Once connected to the container, you can start the Shipping service by running:

```bash
./start.sh
```

You should see the message: "Shipping Service Started. Listening for orders..."

#### 3. Access Prometheus and Grafana

Prometheus will be available at [http://localhost:9090](http://localhost:9090) and Grafana at [http://localhost:3000](http://localhost:3000).

To configure Grafana:
1. Open Grafana in your browser.
2. Add Prometheus as a data source (URL: `http://prometheus:9090`).
3. Create dashboards to visualize metrics for wait time, processing time, and queue depth.

#### 4. Testing the System

The Ordering service will automatically generate orders. Once you've started the Shipping service manually, you should see it processing these orders with messages like:

```
Preparing shipment for order: [OrderId] for [CustomerName]
```

#### 5. Stopping the Demo

To stop all services, press Ctrl+C in the terminal where docker-compose is running, or run:

```bash
docker-compose down -v
```

## Processing Delays

The system now handles processing delays in the following way:

1. **Billing Processing Delay**: This delay is now included in the `OrderPlaced` message and can be configured through the API. The Billing service reads this value from each message and uses it for processing.

2. **Shipping Processing Delay**: This is now a fixed value of 300ms in the Shipping service.

This configuration allows you to experiment with different scenarios by changing the order arrival rate and billing processing delay through the API, while keeping the shipping processing delay constant.

## Further Reading

For more detailed information about the implementation and the metrics used to demonstrate Little's Law, refer to the [PLAN.md](PLAN.md) file.

## Development Notes

- The Shipping service is intentionally configured to require manual startup to demonstrate container shell access
- All services connect to RabbitMQ using the hostname "rabbitmq"
- Each service has its own Dockerfile in its respective directory
- Metrics are exposed via Prometheus endpoints and visualized in Grafana
- The configuration API allows for dynamic adjustment of system parameters without restarting services
