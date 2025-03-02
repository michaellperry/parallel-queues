# WiredBrain Microservices Demo

This project demonstrates a microservices architecture for the WiredBrain coffee shop, consisting of Ordering, Billing, and Shipping services that communicate via RabbitMQ.

## Project Structure

- **WiredBrain.Ordering**: Service that handles customer orders
- **WiredBrain.Billing**: Service that processes payments
- **WiredBrain.Shipping**: Service that prepares shipments
- **WiredBrain.Messages**: Shared message contracts

## Prerequisites

- Docker and Docker Compose
- .NET 9.0 SDK (for development)

## Running the Demo

### 1. Start the Services

From the root directory of the project, run:

```bash
cd mesh
docker-compose up -d --build
```

This will:
- Start a RabbitMQ container
- Build and start the Ordering, Billing, and Shipping service containers

### 2. Manually Start the Shipping Service

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

### 3. Testing the System

The Ordering service will automatically generate orders. Once you've started the Shipping service manually, you should see it processing these orders with messages like:

```
Preparing shipment for order: [OrderId] for [CustomerName]
```

### 4. Stopping the Demo

To stop all services, press Ctrl+C in the terminal where docker-compose is running, or run:

```bash
docker-compose down -v
```

## Development Notes

- The Shipping service is intentionally configured to require manual startup to demonstrate container shell access
- All services connect to RabbitMQ using the hostname "rabbitmq"
- Each service has its own Dockerfile in its respective directory
