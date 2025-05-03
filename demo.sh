#!/bin/bash

# WiredBrain Queueing Theory Demo Script
# This script demonstrates Little's Law and queueing theory concepts using the WiredBrain application

# Color codes for better readability
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Function to print section headers
print_header() {
    echo -e "\n${BLUE}=========================================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}=========================================================${NC}\n"
}

# Function to print scenario information
print_scenario() {
    echo -e "\n${GREEN}=== SCENARIO $1: $2 ===${NC}"
    echo -e "${YELLOW}$3${NC}\n"
}

# Function to print instructions
print_instructions() {
    echo -e "${YELLOW}INSTRUCTIONS:${NC}"
    echo -e "$1\n"
}

# Function to print what to observe
print_observe() {
    echo -e "${YELLOW}WHAT TO OBSERVE:${NC}"
    echo -e "$1\n"
}

# Function to print theoretical explanation
print_theory() {
    echo -e "${YELLOW}THEORY:${NC}"
    echo -e "$1\n"
}

# Function to start services with specific parameters
start_services() {
    local order_rate=$1
    local billing_delay=$2
    local shipping_delay=$3
    
    echo -e "Starting services with the following parameters:"
    echo -e "  - Order Arrival Rate: ${GREEN}$order_rate ms${NC} between orders (λ = $(echo "scale=3; 1000/$order_rate" | bc) orders/sec)"
    echo -e "  - Billing Processing Time: ${GREEN}$billing_delay ms${NC} (τ_billing = $(echo "scale=3; $billing_delay/1000" | bc) sec, service rate = $(echo "scale=3; 1000/$billing_delay" | bc) orders/sec)"
    echo -e "  - Shipping Processing Time: ${GREEN}$shipping_delay ms${NC} (τ_shipping = $(echo "scale=3; $shipping_delay/1000" | bc) sec, service rate = $(echo "scale=3; 1000/$shipping_delay" | bc) orders/sec)"
    
    # Export environment variables for docker-compose
    export ORDER_ARRIVAL_RATE_MS=$order_rate
    export BILLING_PROCESSING_DELAY_MS=$billing_delay
    export SHIPPING_PROCESSING_DELAY_MS=$shipping_delay
    
    # Start services
    cd mesh
    docker-compose down -v
    docker-compose up -d --build
    cd ..
    
    echo -e "\n${GREEN}Services started successfully!${NC}"
    echo -e "Grafana dashboard is available at: ${BLUE}http://localhost:3000${NC}"
    echo -e "Default login: admin/admin"
    echo -e "Prometheus is available at: ${BLUE}http://localhost:9090${NC}"
    echo -e "RabbitMQ Management UI is available at: ${BLUE}http://localhost:15672${NC}"
}

# Function to stop services
stop_services() {
    print_header "Stopping Services"
    cd mesh
    docker-compose down -v
    cd ..
    echo -e "${GREEN}Services stopped successfully!${NC}"
}

# Main menu function
show_menu() {
    clear
    print_header "WiredBrain Queueing Theory Demo"
    echo "This script demonstrates Little's Law (L = λW) and queueing theory concepts"
    echo "using the WiredBrain microservices application."
    echo
    echo "Available scenarios:"
    echo "1. Scenario A: Underload (λ < 1/τ)"
    echo "2. Scenario B: Near Capacity (λ ≈ 1/τ)"
    echo "3. Scenario C: Overload (λ > 1/τ)"
    echo "4. Scenario D: Impact of Service Time"
    echo "5. Scenario E: Bottleneck Identification"
    echo "6. Stop all services"
    echo "0. Exit"
    echo
    read -p "Enter your choice [0-6]: " choice
    
    case $choice in
        1) scenario_a ;;
        2) scenario_b ;;
        3) scenario_c ;;
        4) scenario_d ;;
        5) scenario_e ;;
        6) stop_services; show_menu ;;
        0) stop_services; exit 0 ;;
        *) echo -e "${RED}Invalid option. Please try again.${NC}"; sleep 2; show_menu ;;
    esac
}

# Scenario A: Underload (λ < 1/τ)
scenario_a() {
    print_scenario "A" "Underload (λ < 1/τ)" "Minimal queueing, W ≈ τ"
    
    print_theory "In an underload scenario, the arrival rate (λ) is significantly less than the service rate (1/τ).
This means the system can process orders faster than they arrive, resulting in:
- Minimal or no queueing
- The time in system (W) is approximately equal to the processing time (τ)
- Little's Law still holds: L = λW, but L will be small"
    
    print_instructions "This scenario will configure:
- Order arrival rate: 2000 ms between orders (λ = 0.5 orders/sec)
- Billing processing time: 200 ms (service rate = 5 orders/sec)
- Shipping processing time: 300 ms (service rate = 3.33 orders/sec)

Since λ (0.5) < min(1/τ_billing, 1/τ_shipping) = min(5, 3.33) = 3.33, the system is in underload."
    
    print_observe "In the Grafana dashboard (http://localhost:3000):
1. The queue lengths for both Billing and Shipping should remain very low (near zero)
2. The time in system (W) for both services should be very close to their processing times (τ)
3. Little's Law verification panels should show 'Measured L' and 'Calculated λW' lines nearly overlapping
4. The arrival rate should be well below the processing rates of both services"
    
    # Start services with underload parameters
    start_services 2000 200 300
    
    read -p "Press Enter to return to the main menu..."
    show_menu
}

# Scenario B: Near Capacity (λ ≈ 1/τ)
scenario_b() {
    print_scenario "B" "Near Capacity (λ ≈ 1/τ)" "Increasing queue lengths"
    
    print_theory "In a near capacity scenario, the arrival rate (λ) is close to the service rate (1/τ).
This creates a system that is operating near its maximum throughput:
- Queue lengths start to grow but may stabilize
- The time in system (W) becomes significantly greater than the processing time (τ)
- The difference (W - τ) represents time spent waiting in the queue
- Little's Law becomes more visibly important as L increases"
    
    print_instructions "This scenario will configure:
- Order arrival rate: 350 ms between orders (λ = 2.86 orders/sec)
- Billing processing time: 200 ms (service rate = 5 orders/sec)
- Shipping processing time: 300 ms (service rate = 3.33 orders/sec)

Since λ (2.86) ≈ min(1/τ_billing, 1/τ_shipping) = min(5, 3.33) = 3.33, the system is near capacity."
    
    print_observe "In the Grafana dashboard (http://localhost:3000):
1. The Shipping queue length should start to increase (since it's closer to capacity)
2. The Billing queue should remain relatively small
3. The time in system (W) for Shipping should be noticeably greater than its processing time (τ)
4. Little's Law verification becomes more evident as queue lengths increase
5. The arrival rate is approaching the processing rate of the Shipping service"
    
    # Start services with near capacity parameters
    start_services 350 200 300
    
    read -p "Press Enter to return to the main menu..."
    show_menu
}

# Scenario C: Overload (λ > 1/τ)
scenario_c() {
    print_scenario "C" "Overload (λ > 1/τ)" "Continuously growing queues"
    
    print_theory "In an overload scenario, the arrival rate (λ) exceeds the service rate (1/τ).
This creates an unstable system where:
- Queue lengths grow continuously without bound
- The time in system (W) increases over time
- The system cannot keep up with incoming orders
- Little's Law still holds, but L and W continue to increase over time"
    
    print_instructions "This scenario will configure:
- Order arrival rate: 250 ms between orders (λ = 4 orders/sec)
- Billing processing time: 200 ms (service rate = 5 orders/sec)
- Shipping processing time: 300 ms (service rate = 3.33 orders/sec)

Since λ (4) > min(1/τ_billing, 1/τ_shipping) = min(5, 3.33) = 3.33, the system is in overload."
    
    print_observe "In the Grafana dashboard (http://localhost:3000):
1. The Shipping queue length should grow continuously
2. The Billing queue should remain relatively stable (since it can handle the load)
3. The time in system (W) for Shipping will increase over time
4. Little's Law verification panels should show both 'Measured L' and 'Calculated λW' increasing
5. The arrival rate exceeds the processing rate of the Shipping service"
    
    # Start services with overload parameters
    start_services 250 200 300
    
    read -p "Press Enter to return to the main menu..."
    show_menu
}

# Scenario D: Impact of Service Time
scenario_d() {
    print_scenario "D" "Impact of Service Time" "Changing τ affects queue length"
    
    print_theory "This scenario demonstrates how changing the service time (τ) directly impacts queue length.
According to queueing theory:
- Increasing τ (slower processing) increases queue length (L) and time in system (W)
- Decreasing τ (faster processing) decreases queue length (L) and time in system (W)
- The relationship is non-linear: small increases in τ can cause large increases in L and W when near capacity"
    
    print_instructions "This scenario will configure:
- Order arrival rate: 300 ms between orders (λ = 3.33 orders/sec)
- Billing processing time: 200 ms (service rate = 5 orders/sec)
- Shipping processing time: 500 ms (service rate = 2 orders/sec)

We've increased the Shipping processing time from 300ms to 500ms, making it a clear bottleneck.
Since λ (3.33) > 1/τ_shipping (2), the Shipping service is in overload."
    
    print_observe "In the Grafana dashboard (http://localhost:3000):
1. Compare this to Scenario B or C - notice how the increased processing time dramatically affects queue length
2. The Shipping queue length should grow faster than in previous scenarios
3. The time in system (W) for Shipping will be much higher
4. The Billing service should still handle the load well
5. This demonstrates how critical the service time (τ) is to system performance"
    
    # Start services with modified service time parameters
    start_services 300 200 500
    
    read -p "Press Enter to return to the main menu..."
    show_menu
}

# Scenario E: Bottleneck Identification
scenario_e() {
    print_scenario "E" "Bottleneck Identification" "Slowest service becomes the bottleneck"
    
    print_theory "This scenario demonstrates how the slowest service in a chain becomes the bottleneck:
- In a series of queues, the service with the highest τ (lowest service rate) becomes the bottleneck
- The bottleneck determines the maximum throughput of the entire system
- Improving non-bottleneck services has minimal impact on overall system performance
- This is a key principle in the Theory of Constraints"
    
    print_instructions "This scenario will configure:
- Order arrival rate: 300 ms between orders (λ = 3.33 orders/sec)
- Billing processing time: 400 ms (service rate = 2.5 orders/sec)
- Shipping processing time: 200 ms (service rate = 5 orders/sec)

We've reversed the processing times, making Billing slower than Shipping.
Since λ (3.33) > 1/τ_billing (2.5), the Billing service is now the bottleneck."
    
    print_observe "In the Grafana dashboard (http://localhost:3000):
1. The Billing queue length should grow continuously
2. The Shipping queue should remain small despite the high arrival rate
3. This is because orders are being throttled by the Billing service
4. The time in system (W) for Billing will increase over time
5. This demonstrates how the bottleneck service controls the flow through the entire system"
    
    # Start services with bottleneck identification parameters
    start_services 300 400 200
    
    read -p "Press Enter to return to the main menu..."
    show_menu
}

# Start the script
show_menu