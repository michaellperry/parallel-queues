# WiredBrain Queueing Theory Demo: Grafana Dashboard Setup

This document provides step-by-step instructions for creating a Grafana dashboard to visualize Little's Law (L = λW) in the WiredBrain application.

## Prerequisites

- The WiredBrain application is running with all services (Ordering, Billing, Shipping, RabbitMQ, Prometheus, and Grafana)
- Grafana is accessible at http://localhost:3000
- Default login credentials: admin/admin

## Setting Up the Prometheus Data Source

1. Log in to Grafana
2. Go to Configuration (gear icon) > Data sources
3. Click "Add data source"
4. Select "Prometheus"
5. Set the URL to `http://prometheus:9090`
6. Click "Save & Test" to verify the connection

## Creating the Dashboard

### Dashboard Settings

1. Click the "+" icon in the sidebar and select "Dashboard"
2. Click the gear icon in the top right to open dashboard settings
3. Set the following:
   - Name: "WiredBrain Queueing Theory Demo"
   - Description: "Visualizing Little's Law (L = λW) in a microservices architecture"
   - Tags: "queueing-theory", "littles-law"
4. Click "Save dashboard" in the top right

### Adding Introduction Text Panel

1. Click "Add panel" (+ icon)
2. Select "Add a new panel" > "Text"
3. In the Text edit mode, select "Markdown" and enter:
   ```markdown
   # WiredBrain Queueing Theory Demo: Little's Law

   This dashboard demonstrates Little's Law (L = λW), a fundamental principle in queueing theory that states:

   **The average number of items in a system (L) equals the average arrival rate (λ) multiplied by the average time an item spends in the system (W).**
   ```
4. In the Panel options (right side):
   - Title: "Introduction"
   - Transparent background: Enabled
5. Click "Apply"

### Creating System Overview Row

1. Click "Add panel" (+ icon)
2. Select "Add a new row"
3. Set the title to "System Overview"
4. Click "Create"

### Adding Arrival Rate Panel

1. Click "Add panel" (+ icon) within the System Overview row
2. Select "Add a new panel" > "Time series"
3. In the Query tab:
   - Data source: Prometheus
   - Enter the query: `rate(orders_placed_total[5m])`
   - Legend: `Arrival Rate (λ)`
4. In the Panel options:
   - Title: "Arrival Rate (λ)"
   - Description: "The rate at which new orders are being placed (λ)"
5. In the Field tab:
   - Unit: "orders/sec" (custom unit)
   - Min: 0
6. Click "Apply"

### Adding Orders Processed Panel

1. Click "Add panel" (+ icon)
2. Select "Add a new panel" > "Time series"
3. In the Query tab:
   - Data source: Prometheus
   - Add two queries:
     - A: `billing_processed_total` with legend "Billing Processed"
     - B: `shipping_processed_total` with legend "Shipping Processed"
4. In the Panel options:
   - Title: "Orders Processed"
   - Description: "The number of orders processed by each service"
5. Click "Apply"

### Adding Current Queue Lengths Panel

1. Click "Add panel" (+ icon)
2. Select "Add a new panel" > "Gauge"
3. In the Query tab:
   - Data source: Prometheus
   - Add two queries:
     - A: `rabbitmq_queue_messages{queue="billing-service"}` with legend "Billing Queue"
     - B: `rabbitmq_queue_messages{queue="shipping-service"}` with legend "Shipping Queue"
4. In the Panel options:
   - Title: "Current Queue Lengths"
   - Description: "Current number of messages in each queue"
5. In the Field tab:
   - Unit: "messages" (custom unit)
   - Thresholds:
     - 0-10: green
     - 10-50: yellow
     - 50+: red
6. Click "Apply"

### Adding Current Processing Rates Panel

1. Click "Add panel" (+ icon)
2. Select "Add a new panel" > "Gauge"
3. In the Query tab:
   - Data source: Prometheus
   - Add two queries:
     - A: `rate(billing_processed_total[1m])` with legend "Billing Rate"
     - B: `rate(shipping_processed_total[1m])` with legend "Shipping Rate"
4. In the Panel options:
   - Title: "Current Processing Rates"
   - Description: "Current processing rates for each service"
5. In the Field tab:
   - Unit: "orders/sec" (custom unit)
   - Thresholds:
     - 0-1: red
     - 1-2: yellow
     - 2+: green
6. Click "Apply"

### Creating Billing Service Metrics Row

1. Click "Add panel" (+ icon)
2. Select "Add a new row"
3. Set the title to "Billing Service Metrics"
4. Click "Create"

### Adding Billing Service Explanation Text

1. Click "Add panel" (+ icon) within the Billing Service Metrics row
2. Select "Add a new panel" > "Text"
3. In the Text edit mode, select "Markdown" and enter:
   ```markdown
   ## Billing Service Metrics

   These panels show the key metrics for the Billing service that are used to demonstrate Little's Law (L = λW):

   - **Processing Time (τ)**: The average time it takes to process a single order
   - **Time in System (W)**: The average time an order spends in the system (includes both queue time and processing time)
   - **Queue Length (L)**: The average number of orders in the queue
   - **Little's Law Verification**: Comparing the measured queue length (L) with the calculated value (λW)
   ```
4. In the Panel options:
   - Title: (leave blank)
   - Transparent background: Enabled
5. Click "Apply"

### Adding Billing Processing Time Panel

1. Click "Add panel" (+ icon)
2. Select "Add a new panel" > "Time series"
3. In the Query tab:
   - Data source: Prometheus
   - Enter the query: `rate(billing_processing_time_seconds_sum[5m]) / rate(billing_processing_time_seconds_count[5m])`
   - Legend: `Processing Time (τ)`
4. In the Panel options:
   - Title: "Billing Processing Time (τ)"
   - Description: "Average time it takes to process a single order in the Billing service"
5. In the Field tab:
   - Unit: "seconds (s)"
   - Min: 0
6. Click "Apply"

### Adding Billing Time in System Panel

1. Click "Add panel" (+ icon)
2. Select "Add a new panel" > "Time series"
3. In the Query tab:
   - Data source: Prometheus
   - Enter the query: `rate(billing_wait_time_seconds_sum[5m]) / rate(billing_wait_time_seconds_count[5m])`
   - Legend: `Time in System (W)`
4. In the Panel options:
   - Title: "Billing Time in System (W)"
   - Description: "Average time an order spends in the Billing system (includes both queue time and processing time)"
5. In the Field tab:
   - Unit: "seconds (s)"
   - Min: 0
6. Click "Apply"

### Adding Billing Queue Length Panel

1. Click "Add panel" (+ icon)
2. Select "Add a new panel" > "Time series"
3. In the Query tab:
   - Data source: Prometheus
   - Enter the query: `avg_over_time(rabbitmq_queue_messages{queue="billing-service"}[5m])`
   - Legend: `Queue Length (L)`
4. In the Panel options:
   - Title: "Billing Queue Length (L)"
   - Description: "Average number of orders in the Billing queue"
5. In the Field tab:
   - Unit: "messages" (custom unit)
   - Min: 0
6. Click "Apply"

### Adding Billing Little's Law Verification Panel

1. Click "Add panel" (+ icon)
2. Select "Add a new panel" > "Time series"
3. In the Query tab:
   - Data source: Prometheus
   - Add two queries:
     - A: `avg_over_time(rabbitmq_queue_messages{queue="billing-service"}[5m])` with legend "Measured L"
     - B: `rate(orders_placed_total[5m]) * (rate(billing_wait_time_seconds_sum[5m]) / rate(billing_wait_time_seconds_count[5m]))` with legend "Calculated λW"
4. In the Panel options:
   - Title: "Billing Little's Law Verification (L = λW)"
   - Description: "Comparing the measured queue length (L) with the calculated value (λW) to verify Little's Law"
5. In the Field tab:
   - Unit: "messages" (custom unit)
   - Min: 0
6. Click "Apply"

### Creating Shipping Service Metrics Row

1. Click "Add panel" (+ icon)
2. Select "Add a new row"
3. Set the title to "Shipping Service Metrics"
4. Click "Create"

### Adding Shipping Service Explanation Text

1. Click "Add panel" (+ icon) within the Shipping Service Metrics row
2. Select "Add a new panel" > "Text"
3. In the Text edit mode, select "Markdown" and enter:
   ```markdown
   ## Shipping Service Metrics

   These panels show the key metrics for the Shipping service that are used to demonstrate Little's Law (L = λW):

   - **Processing Time (τ)**: The average time it takes to process a single order
   - **Time in System (W)**: The average time an order spends in the system (includes both queue time and processing time)
   - **Queue Length (L)**: The average number of orders in the queue
   - **Little's Law Verification**: Comparing the measured queue length (L) with the calculated value (λW)
   ```
4. In the Panel options:
   - Title: (leave blank)
   - Transparent background: Enabled
5. Click "Apply"

### Adding Shipping Processing Time Panel

1. Click "Add panel" (+ icon)
2. Select "Add a new panel" > "Time series"
3. In the Query tab:
   - Data source: Prometheus
   - Enter the query: `rate(shipping_processing_time_seconds_sum[5m]) / rate(shipping_processing_time_seconds_count[5m])`
   - Legend: `Processing Time (τ)`
4. In the Panel options:
   - Title: "Shipping Processing Time (τ)"
   - Description: "Average time it takes to process a single order in the Shipping service"
5. In the Field tab:
   - Unit: "seconds (s)"
   - Min: 0
6. Click "Apply"

### Adding Shipping Time in System Panel

1. Click "Add panel" (+ icon)
2. Select "Add a new panel" > "Time series"
3. In the Query tab:
   - Data source: Prometheus
   - Enter the query: `rate(shipping_wait_time_seconds_sum[5m]) / rate(shipping_wait_time_seconds_count[5m])`
   - Legend: `Time in System (W)`
4. In the Panel options:
   - Title: "Shipping Time in System (W)"
   - Description: "Average time an order spends in the Shipping system (includes both queue time and processing time)"
5. In the Field tab:
   - Unit: "seconds (s)"
   - Min: 0
6. Click "Apply"

### Adding Shipping Queue Length Panel

1. Click "Add panel" (+ icon)
2. Select "Add a new panel" > "Time series"
3. In the Query tab:
   - Data source: Prometheus
   - Enter the query: `avg_over_time(rabbitmq_queue_messages{queue="shipping-service"}[5m])`
   - Legend: `Queue Length (L)`
4. In the Panel options:
   - Title: "Shipping Queue Length (L)"
   - Description: "Average number of orders in the Shipping queue"
5. In the Field tab:
   - Unit: "messages" (custom unit)
   - Min: 0
6. Click "Apply"

### Adding Shipping Little's Law Verification Panel

1. Click "Add panel" (+ icon)
2. Select "Add a new panel" > "Time series"
3. In the Query tab:
   - Data source: Prometheus
   - Add two queries:
     - A: `avg_over_time(rabbitmq_queue_messages{queue="shipping-service"}[5m])` with legend "Measured L"
     - B: `rate(orders_placed_total[5m]) * (rate(shipping_wait_time_seconds_sum[5m]) / rate(shipping_wait_time_seconds_count[5m]))` with legend "Calculated λW"
4. In the Panel options:
   - Title: "Shipping Little's Law Verification (L = λW)"
   - Description: "Comparing the measured queue length (L) with the calculated value (λW) to verify Little's Law"
5. In the Field tab:
   - Unit: "messages" (custom unit)
   - Min: 0
6. Click "Apply"

## Final Steps

1. Click "Save dashboard" in the top right
2. Set the refresh rate to "10s" using the dropdown in the top right
3. Adjust the time range to "Last 15 minutes" using the time picker in the top right

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
2. **Near Capacity (λ ≈ 1/τ)**: Increase arrival rate to be close to service rate and observe growing queues
3. **Overload (λ > 1/τ)**: Set arrival rate higher than service rate and observe continuously growing queues
4. **Bottleneck Identification**: Set different processing times for Billing and Shipping to see which becomes the bottleneck