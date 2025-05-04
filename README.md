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

### Kingman's Formula and Variability

While Little's Law (L = λW) is a fundamental relationship that holds true for any stable system, it doesn't tell us *why* queue lengths and wait times change. Kingman's Formula, also known as the V-UT equation (Variability * Utilization * Time), provides insight into the factors that influence waiting time in a queue, particularly the impact of variability in arrival and service times.

For a single-server queue (M/G/1), Kingman's Formula for the average waiting time in the queue (Wq) is:

Wq ≈ (ca² + cs²) / 2 * (ρ / (1-ρ)) * τ

Where:
- ca² is the squared coefficient of variation of arrival times (a measure of arrival variability)
- cs² is the squared coefficient of variation of service times (a measure of service variability)
- ρ is the utilization (λ/μ)
- τ is the average service time

The formula highlights that waiting time is proportional to:
- **Variability (ca² + cs²)**: Higher variability in either arrivals or service times leads to longer wait times.
- **Utilization (ρ / (1-ρ))**: As utilization approaches 1 (100%), the term ρ / (1-ρ) approaches infinity, causing wait times to increase dramatically. This demonstrates the non-linear relationship between utilization and waiting time.
- **Average Service Time (τ)**: Longer service times directly increase waiting times.

#### Modeling Service Times with the Gamma Distribution

In real-world systems, service times are rarely constant. They often exhibit variability. To model this variability in the WiredBrain demo, the Billing service uses the Gamma distribution to simulate processing times.

The Gamma distribution is a flexible continuous probability distribution that can model a wide range of waiting times. It is particularly appropriate here because:
- It only produces positive values, which is necessary for modeling time.
- Its shape can be adjusted using two parameters: the shape parameter (k) and the scale parameter (θ).
- By adjusting these parameters, we can control the mean and variance of the service times, and thus the coefficient of variation (cs).

The Billing service uses the MathNet.Numerics library to generate random numbers following a Gamma distribution with configurable parameters. The relationship between the Gamma distribution parameters (shape k, scale θ), the mean (μ_gamma), variance (σ²_gamma), and coefficient of variation (cs) is:
- μ_gamma = k * θ
- σ²_gamma = k * θ²
- cs = σ_gamma / μ_gamma = sqrt(k * θ²) / (k * θ) = sqrt(k) * θ / (k * θ) = 1 / sqrt(k)

This shows that the coefficient of service variation (cs) is determined solely by the shape parameter (k) of the Gamma distribution.

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
- `billingServiceCount` (optional) is the number of billing services available (default: 16)

The maximum service rate is calculated as cμ, where:
- c is the number of services (billingServiceCount)
- μ is the service rate per service (1/τ, where τ is the processing time in seconds)

For example, with a billing processing delay of 500ms (τ = 0.5s), the service rate per service (μ) is 2 orders/sec. With 16 services (c = 16), the maximum service rate (cμ) becomes 32 orders/sec.

Available scenarios:

1. **underload**: Demonstrates a system under minimal load
   - OrderArrivalRateMs: 4800 / BillingServiceCount (λ = 4.8 orders/sec per service)
   - BillingProcessingDelayMs: 500 (μ = 2 orders/sec per service)
   - Default BillingServiceCount: 16 (cμ = 32 orders/sec)
   - Result: Minimal queueing, W ≈ τ

2. **nearcapacity**: Demonstrates a system operating near its capacity
   - OrderArrivalRateMs: 625 / BillingServiceCount (λ = 1.6 orders/sec per service)
   - BillingProcessingDelayMs: 500 (μ = 2 orders/sec per service)
   - Default BillingServiceCount: 16 (cμ = 32 orders/sec)
   - Result: Increasing queue lengths when c is reduced

3. **overload**: Demonstrates a system under excessive load
   - OrderArrivalRateMs: 450 / BillingServiceCount (λ = 2.2 orders/sec per service)
   - BillingProcessingDelayMs: 500 (μ = 2 orders/sec per service)
   - Default BillingServiceCount: 16 (cμ = 32 orders/sec)
   - Result: Continuously growing queues when c is reduced

4. **lowvariability**: Demonstrates Low Variability, Moderate Utilization
   - ca = 0.5 (CoefficientOfArrivalVariation)
   - cs = 0.5 (CoefficientOfServiceVariation)
   - ρ ≈ 0.7 (utilization)
   - BillingProcessingDelayMs: 500 (μ = 2 orders/sec per service)
   - Default BillingServiceCount: 16
   - Result: Moderate queueing and wait times with low variability

5. **highservicevariability**: Demonstrates High Service Variability, Moderate Utilization
   - ca = 0.5 (CoefficientOfArrivalVariation)
   - cs = 2.0 (CoefficientOfServiceVariation)
   - ρ ≈ 0.7 (utilization)
   - BillingProcessingDelayMs: 500 (μ = 2 orders/sec per service)
   - Default BillingServiceCount: 16
   - Result: Longer queueing and wait times due to service variability

6. **highvariability**: Demonstrates High Arrival and Service Variability, High Utilization
   - ca = 1.5 (CoefficientOfArrivalVariation)
   - cs = 1.5 (CoefficientOfServiceVariation)
   - ρ ≈ 0.9 (utilization)
   - BillingProcessingDelayMs: 500 (μ = 2 orders/sec per service)
   - Default BillingServiceCount: 16
   - Result: Very long and highly volatile queueing and wait times

#### Custom Configuration

The configuration API in the WiredBrain.Ordering service allows you to fully customize the queueing behavior, including the coefficients of variation. You can do this by making a POST request to `/api/Configuration` with a JSON body containing the desired `QueueConfiguration`.

The `QueueConfiguration` object allows you to set:
- `OrderArrivalDelayMs`: The delay between order placements in milliseconds (influences arrival rate λ).
- `BillingProcessingDelayMs`: The *mean* billing processing delay in milliseconds (influences average service time τ).
- `BillingServiceCount`: The number of concurrent billing services (influences number of servers c).
- `CoefficientOfArrivalVariation`: The coefficient of variation for arrival times (ca).
- `CoefficientOfServiceVariation`: The coefficient of variation for service times (cs).

By adjusting `CoefficientOfArrivalVariation` and `CoefficientOfServiceVariation`, you can directly experiment with the impact of variability on the system. Note that the Billing service internally calculates the necessary Gamma distribution parameters (shape and scale) based on the provided `BillingProcessingDelayMs` (mean) and `CoefficientOfServiceVariation` (cs) to simulate service times with the desired characteristics.

Example JSON body:

```json
{
  "orderArrivalDelayMs": 500,
  "billingProcessingDelayMs": 300,
  "billingServiceCount": 2,
  "coefficientOfArrivalVariation": 0.5,
  "coefficientOfServiceVariation": 1.0
}
```

**Experimenting with Variability Scenarios (Custom Configuration)**

To demonstrate the impact of variability using Kingman's Formula, you can use the `POST /api/Configuration` endpoint with custom configurations. Here are a few examples:

1.  **Scenario: Low Variability, Moderate Utilization (ca ≈ 0.5, cs ≈ 0.5, ρ ≈ 0.7)**
    -   Description: This scenario shows the system behavior with relatively low variability in both arrivals and service times, operating at moderate utilization.
    -   Configuration JSON:
        ```json
        {
          "orderArrivalDelayMs": 350,
          "billingProcessingDelayMs": 500,
          "billingServiceCount": 16,
          "coefficientOfArrivalVariation": 0.5,
          "coefficientOfServiceVariation": 0.5
        }
        ```
    -   Expected Behavior: Moderate queueing and wait times. The system should remain stable, and observed wait times will be reasonably close to the predictions of Kingman's formula for these coefficient values.

2.  **Scenario: High Service Variability, Moderate Utilization (ca ≈ 0.5, cs ≈ 2.0, ρ ≈ 0.7)**
    -   Description: This scenario highlights the impact of high variability in service times while keeping arrival variability and utilization the same as the previous scenario.
    -   Configuration JSON:
        ```json
        {
          "orderArrivalDelayMs": 350,
          "billingProcessingDelayMs": 500,
          "billingServiceCount": 16,
          "coefficientOfArrivalVariation": 0.5,
          "coefficientOfServiceVariation": 2.0
        }
        ```
    -   Expected Behavior: Significantly longer queueing and wait times compared to the low variability scenario, even though utilization is the same. The dashboard will show more volatile queue lengths and wait times, demonstrating the strong influence of `cs` when utilization is not very low.

3.  **Scenario: High Arrival and Service Variability, High Utilization (ca ≈ 1.5, cs ≈ 1.5, ρ ≈ 0.9)**
    -   Description: This scenario demonstrates the combined effect of high variability in both arrivals and service times under high utilization.
    -   Configuration JSON:
        ```json
        {
          "orderArrivalDelayMs": 250,
          "billingProcessingDelayMs": 500,
          "billingServiceCount": 16,
          "coefficientOfArrivalVariation": 1.5,
          "coefficientOfServiceVariation": 1.5
        }
        ```
    -   Expected Behavior: Very long and highly volatile queueing and wait times. The system will be very sensitive to any fluctuations, potentially showing signs of instability. The dashboard will clearly show the dramatic increase in wait times predicted by Kingman's formula as both variability and utilization are high.

*(Note: All predefined scenarios are available via `/api/Configuration/scenario/{scenarioName}`. The first three scenarios (`underload`, `nearcapacity`, `overload`) set `ca` and `cs` to 0, while the new scenarios (`lowvariability`, `highservicevariability`, `highvariability`) set these coefficients to specific values to demonstrate the impact of variability as described above.)*

#### Dashboard Expectations

When running these scenarios and observing the Grafana dashboard, you should expect to see:

-   **Queue Lengths**: Under low utilization and variability, queue lengths will be minimal. As utilization and/or variability increase, queue lengths will grow, becoming more volatile with higher variability.
-   **Wait Times**: Similar to queue lengths, wait times will be short in low utilization/variability scenarios and increase significantly as these factors rise. The impact of variability will be evident in the spread and fluctuations of wait times.
-   **Utilization**: The utilization metric (ρ) will show how busy the billing services are. As ρ approaches 1, you will see the dramatic increase in wait times predicted by Kingman's Formula, an effect amplified by higher variability.

The mathematical relationship: Wait time ≈ (ca² + cs²) / 2 * (ρ / (1-ρ)) * service time will be visually demonstrated by the observed wait times on the dashboard correlating with the configured variability (ca and cs), utilization (ρ), and average service time.

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
