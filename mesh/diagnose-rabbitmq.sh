#!/bin/bash

echo "=== RabbitMQ Diagnostic Script ==="
echo

echo "1. Checking if RabbitMQ container is running..."
CONTAINER_ID=$(docker ps -qf "name=rabbitmq")
if [ -z "$CONTAINER_ID" ]; then
    echo "RabbitMQ container is not running. Checking if it exists..."
    CONTAINER_ID=$(docker ps -aqf "name=rabbitmq")
    if [ -z "$CONTAINER_ID" ]; then
        echo "No RabbitMQ container found. Please start the services first."
        exit 1
    else
        echo "RabbitMQ container exists but is not running."
        echo "Container ID: $CONTAINER_ID"
        echo
        echo "2. Checking container logs..."
        docker logs $CONTAINER_ID
        echo
        echo "3. Checking container exit code..."
        docker inspect $CONTAINER_ID --format='{{.State.ExitCode}}'
        echo
        echo "4. Checking container status..."
        docker inspect $CONTAINER_ID --format='{{.State.Status}}'
        echo
        echo "5. Checking container health..."
        docker inspect $CONTAINER_ID --format='{{.State.Health.Status}}'
        echo
        echo "6. Checking last error..."
        docker inspect $CONTAINER_ID --format='{{.State.Error}}'
        echo
        echo "7. Checking restart count..."
        docker inspect $CONTAINER_ID --format='{{.RestartCount}}'
        echo
        echo "8. Checking detailed container state..."
        docker inspect $CONTAINER_ID --format='{{json .State}}' | jq
        echo
        echo "9. Checking last 50 log lines..."
        docker logs --tail 50 $CONTAINER_ID
    fi
else
    echo "RabbitMQ container is running."
    echo "Container ID: $CONTAINER_ID"
    echo
    echo "2. Checking container logs..."
    docker logs $CONTAINER_ID
    echo
    echo "3. Checking RabbitMQ status..."
    docker exec $CONTAINER_ID rabbitmqctl status || echo "Failed to get RabbitMQ status"
    echo
    echo "4. Checking enabled plugins..."
    docker exec $CONTAINER_ID rabbitmq-plugins list -e || echo "Failed to list enabled plugins"
    echo
    echo "5. Checking memory usage..."
    docker stats $CONTAINER_ID --no-stream
    echo
    echo "6. Checking disk space..."
    docker exec $CONTAINER_ID df -h || echo "Failed to check disk space"
    echo
    echo "7. Checking RabbitMQ environment..."
    docker exec $CONTAINER_ID rabbitmqctl environment || echo "Failed to get RabbitMQ environment"
    echo
    echo "8. Checking RabbitMQ config file..."
    docker exec $CONTAINER_ID cat /etc/rabbitmq/rabbitmq.conf || echo "Failed to read config file"
    echo
    echo "9. Checking detailed container state..."
    docker inspect $CONTAINER_ID --format='{{json .State}}' | jq
fi

echo
echo "=== Diagnostic Complete ==="