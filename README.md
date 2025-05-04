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
5. **Multiple Servers (c)**: How having multiple service instances affects the maximum service rate (cμ)
6. **Time in Queue vs. Time in System**: The distinction between waiting time and total time

## Running the Demo

### Configuration API

The WiredBrain.Ordering service now provides a REST API for configuring the queueing behavior of the system. This allows you to experiment with different scenarios without restarting the services.

#### Using the Swagger UI

The easiest way to interact with the configuration API is through the Swagger UI:

1. Start the services as described below
2. Open a browser and navigate to `http://localhost:8081/swagger` (or the appropriate port if different)
3. You'll see the Swagger UI with the available endpoints:
   - `GET /api/Configuration` - Get the current configuration
   - `POST /api/Configuration` - Update the configuration manually
   - `POST /api/Configuration/scenario/{scenarioName}` - Apply a predefined scenario

#### Available Scenarios

You can quickly apply predefined scenarios by making a POST request to `/api/Configuration/scenario/{scenarioName}?billingServiceCount={count}` where:
- `scenarioName` is one of the predefined scenarios listed below
- `billingServiceCount` (optional) is the number of billing services available (default: 4)

The maximum service rate is calculated as cμ, where:
- c is the number of services (billingServiceCount)
- μ is the service rate per service (1/τ, where τ is the processing time in seconds)

For example, with a billing processing delay of 200ms (τ = 0.2s), the service rate per service (μ) is 5 orders/sec. With 4 services (c = 4), the maximum service rate (cμ) becomes 20 orders/sec.

Available scenarios:

1. **underload**: Demonstrates a system under minimal load
   - OrderArrivalRateMs: 2000 (λ = 0.5 orders/sec)
   - BillingProcessingDelayMs: 200 (μ = 5 orders/sec per service)
   - Default BillingServiceCount: 4 (cμ = 20 orders/sec)
   - Result: Minimal queueing, W ≈ τ

2. **nearcapacity**: Demonstrates a system operating near its capacity
   - OrderArrivalRateMs: 350 (λ = 2.86 orders/sec)
   - BillingProcessingDelayMs: 200 (μ = 5 orders/sec per service)
   - Default BillingServiceCount: 4 (cμ = 20 orders/sec)
   - Result: Increasing queue lengths when c is reduced

3. **overload**: Demonstrates a system under excessive load
   - OrderArrivalRateMs: 250 (λ = 4 orders/sec)
   - BillingProcessingDelayMs: 200 (μ = 5 orders/sec per service)
   - Default BillingServiceCount: 4 (cμ = 20 orders/sec)
   - Result: Continuously growing queues when c is reduced

#### Custom Configuration

You can also set custom values by making a POST request to `/api/Configuration` with a JSON body:

```json
{
  "orderArrivalRateMs": 500,
  "billingProcessingDelayMs": 300,
  "billingServiceCount": 2
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
- Build and start the Ordering, Billing, and Shipping service containers (with Billing constrained to 2 CPUs)
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

## Processing Delays and Service Capacity

The system now handles processing delays and service capacity in the following way:

1. **Billing Processing Delay**: This delay is included in the `OrderPlaced` message and can be configured through the API. The Billing service reads this value from each message and uses it for processing. This represents τ in the service rate formula μ = 1/τ.

2. **Billing Service Count**: This represents the number of concurrent billing services (c) available to process orders. The maximum service rate is calculated as cμ, where μ = 1/τ. This can be configured through the API.

3. **Shipping Processing Delay**: This is a fixed value of 300ms in the Shipping service.

This configuration allows you to experiment with different scenarios by changing:
- The order arrival rate (λ)
- The billing processing delay (τ)
- The number of billing services (c)

The Billing service is constrained to 2 CPUs in the docker-compose configuration, which can affect the actual number of concurrent messages that can be processed.

## Further Reading

For more detailed information about the implementation and the metrics used to demonstrate Little's Law, refer to the [PLAN.md](PLAN.md) file.

## Development Notes

- The Shipping service is intentionally configured to require manual startup to demonstrate container shell access
- All services connect to RabbitMQ using the hostname "rabbitmq"
- Each service has its own Dockerfile in its respective directory
- Metrics are exposed via Prometheus endpoints and visualized in Grafana
- The configuration API allows for dynamic adjustment of system parameters without restarting services
