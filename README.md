# WiredBrain Microservices Demo: Little's Law and Queueing Theory

This project demonstrates a microservices architecture for the WiredBrain coffee shop, consisting of Ordering, Billing, and Shipping services that communicate via RabbitMQ. It has been enhanced to demonstrate Little's Law and fundamental queueing theory concepts using metrics, Prometheus, and Grafana.

## Project Structure

- **WiredBrain.Ordering**: Service that handles customer orders
- **WiredBrain.Billing**: Service that processes payments
- **WiredBrain.Shipping**: Service that prepares shipments
- **WiredBrain.Messages**: Shared message contracts
- **mesh**: Infrastructure components (RabbitMQ, Prometheus, Grafana)
- **demo.sh**: Script to demonstrate Little's Law scenarios

## Prerequisites

- Docker and Docker Compose
- .NET 9.0 SDK (for development)
- Bash shell (for running the demo script)

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

### Using the Demo Script

The easiest way to explore the concepts is to use the provided demo script:

```bash
./demo.sh
```

This interactive script allows you to:
1. Run predefined scenarios demonstrating different queueing theory concepts
2. See explanations of what to observe in each scenario
3. Understand the theoretical principles behind each demonstration

### Scenarios Included

1. **Scenario A (Underload)**: λ < 1/τ - Minimal queueing, W ≈ τ
2. **Scenario B (Near Capacity)**: λ ≈ 1/τ - Increasing queue lengths
3. **Scenario C (Overload)**: λ > 1/τ - Continuously growing queues
4. **Scenario D (Impact of Service Time)**: Shows how changing τ affects queue length
5. **Scenario E (Bottleneck Identification)**: Demonstrates how the slowest service becomes the bottleneck

### Manual Setup

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

## Customizing the Demo

You can modify the following environment variables in the docker-compose.yml file to experiment with different scenarios:

- `ORDER_ARRIVAL_RATE_MS`: Controls the arrival rate (λ)
- `BILLING_PROCESSING_DELAY_MS`: Controls the billing processing time (τ)
- `SHIPPING_PROCESSING_DELAY_MS`: Controls the shipping processing time (τ)

## Further Reading

For more detailed information about the implementation and the metrics used to demonstrate Little's Law, refer to the [PLAN.md](PLAN.md) file.

## Development Notes

- The Shipping service is intentionally configured to require manual startup to demonstrate container shell access
- All services connect to RabbitMQ using the hostname "rabbitmq"
- Each service has its own Dockerfile in its respective directory
- Metrics are exposed via Prometheus endpoints and visualized in Grafana
