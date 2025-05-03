# WiredBrain Queueing Theory Demo: Grafana Dashboard

This directory contains the configuration for the Grafana dashboard that visualizes Little's Law (L = λW) in the WiredBrain application.

## Directory Structure

- `dashboards/`: Contains the dashboard JSON file
- `provisioning/`: Contains the Grafana provisioning configuration
  - `dashboards/`: Dashboard auto-provisioning configuration
  - `datasources/`: Data source auto-provisioning configuration
- `DASHBOARD_SETUP.md`: Step-by-step instructions for manually creating the dashboard in the Grafana UI

## Accessing the Dashboard

1. Start the WiredBrain application using Docker Compose:
   ```
   cd mesh
   docker-compose up -d
   ```

2. Access Grafana at http://localhost:3000
   - Username: admin
   - Password: admin

3. The "WiredBrain Queueing Theory Demo" dashboard should be automatically provisioned and available in the dashboard list.

## Understanding the Dashboard

The dashboard is organized into three main sections:

1. **System Overview**: Shows high-level metrics for the entire system
   - Arrival Rate (λ): The rate at which new orders are being placed
   - Orders Processed: The number of orders processed by each service
   - Current Queue Lengths: The current number of messages in each queue
   - Current Processing Rates: The current processing rates for each service

2. **Billing Service Metrics**: Shows detailed metrics for the Billing service
   - Processing Time (τ): The average time it takes to process a single order
   - Time in System (W): The average time an order spends in the system
   - Queue Length (L): The average number of orders in the queue
   - Little's Law Verification: Comparing the measured queue length (L) with the calculated value (λW)

3. **Shipping Service Metrics**: Shows detailed metrics for the Shipping service
   - Processing Time (τ): The average time it takes to process a single order
   - Time in System (W): The average time an order spends in the system
   - Queue Length (L): The average number of orders in the queue
   - Little's Law Verification: Comparing the measured queue length (L) with the calculated value (λW)

## Demonstrating Little's Law

To effectively demonstrate Little's Law using this dashboard:

1. Observe the relationship between Arrival Rate (λ), Time in System (W), and Queue Length (L)
2. Compare the "Measured L" and "Calculated λW" values in the verification panels
3. Experiment with different arrival rates and processing delays by modifying the environment variables in docker-compose.yml:
   - `ORDER_ARRIVAL_RATE_MS`: Controls the arrival rate (λ)
   - `BILLING_PROCESSING_DELAY_MS`: Controls the billing processing time (τ)
   - `SHIPPING_PROCESSING_DELAY_MS`: Controls the shipping processing time (τ)

### Scenarios to Try

1. **Underload (λ < 1/τ)**: Set a low arrival rate and observe minimal queueing
   ```yaml
   environment:
     - ORDER_ARRIVAL_RATE_MS=2000  # 1 order every 2 seconds
     - BILLING_PROCESSING_DELAY_MS=200
     - SHIPPING_PROCESSING_DELAY_MS=300
   ```

2. **Near Capacity (λ ≈ 1/τ)**: Increase arrival rate to be close to service rate
   ```yaml
   environment:
     - ORDER_ARRIVAL_RATE_MS=300  # 1 order every 0.3 seconds
     - BILLING_PROCESSING_DELAY_MS=200
     - SHIPPING_PROCESSING_DELAY_MS=300
   ```

3. **Overload (λ > 1/τ)**: Set arrival rate higher than service rate
   ```yaml
   environment:
     - ORDER_ARRIVAL_RATE_MS=100  # 1 order every 0.1 seconds
     - BILLING_PROCESSING_DELAY_MS=200
     - SHIPPING_PROCESSING_DELAY_MS=300
   ```

4. **Bottleneck Identification**: Set different processing times for Billing and Shipping
   ```yaml
   environment:
     - ORDER_ARRIVAL_RATE_MS=500  # 1 order every 0.5 seconds
     - BILLING_PROCESSING_DELAY_MS=600  # Billing becomes the bottleneck
     - SHIPPING_PROCESSING_DELAY_MS=300
   ```

## Manual Dashboard Creation

If you need to create or modify the dashboard manually, follow the step-by-step instructions in [DASHBOARD_SETUP.md](./DASHBOARD_SETUP.md).