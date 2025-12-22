#!/bin/bash

# Script to refresh Swagger/OpenAPI definitions
# This script:
# 1. Starts the backend service
# 2. Waits for it to be ready
# 3. Runs the swagger code generation
# 4. Shuts down the backend

set -e

# Determine the script's directory and project root
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$( cd "$SCRIPT_DIR/.." && pwd )"
BACKEND_DIR="$PROJECT_ROOT/src/backend/ManagementHub.Service"
FRONTEND_DIR="$PROJECT_ROOT/src/frontend"

# Log file for backend output
LOG_FILE="/tmp/refresh_swagger_backend.log"

echo "Starting backend service..."
cd "$BACKEND_DIR"

# Start the backend service in the background, capturing its output
dotnet run > "$LOG_FILE" 2>&1 &
BACKEND_PID=$!

echo "Backend started with PID: $BACKEND_PID"
echo "Waiting for backend to be ready..."

# Monitor the log file for the ready message
TIMEOUT=120  # 2 minutes timeout
ELAPSED=0
READY=false

while [ $ELAPSED -lt $TIMEOUT ]; do
    if [ -f "$LOG_FILE" ]; then
        if grep -q "Application started. Press Ctrl+C to shut down." "$LOG_FILE"; then
            READY=true
            echo "Backend is ready!"
            break
        fi
    fi
    sleep 1
    ELAPSED=$((ELAPSED + 1))
    
    # Check if the process is still running
    if ! kill -0 $BACKEND_PID 2>/dev/null; then
        echo "Error: Backend process died unexpectedly"
        echo "Last log output:"
        tail -20 "$LOG_FILE" 2>/dev/null || echo "No log file found"
        exit 1
    fi
done

if [ "$READY" = false ]; then
    echo "Error: Backend did not start within $TIMEOUT seconds"
    echo "Last log output:"
    tail -20 "$LOG_FILE" 2>/dev/null || echo "No log file found"
    kill $BACKEND_PID 2>/dev/null || true
    exit 1
fi

# Give it a moment to fully initialize
sleep 2

# Run swagger generation
echo "Running swagger code generation..."
cd "$FRONTEND_DIR"
if yarn swaggergen; then
    echo "Swagger generation completed successfully"
    EXIT_CODE=0
else
    echo "Swagger generation failed"
    EXIT_CODE=1
fi

# Shutdown the backend
echo "Shutting down backend service..."
kill $BACKEND_PID 2>/dev/null || true

# Wait for the process to exit
WAIT_COUNT=0
while kill -0 $BACKEND_PID 2>/dev/null && [ $WAIT_COUNT -lt 10 ]; do
    sleep 1
    WAIT_COUNT=$((WAIT_COUNT + 1))
done

# Force kill if it didn't stop gracefully
if kill -0 $BACKEND_PID 2>/dev/null; then
    echo "Backend did not stop gracefully, force killing..."
    kill -9 $BACKEND_PID 2>/dev/null || true
fi

echo "Backend stopped"

# Clean up log file
rm -f "$LOG_FILE"

echo "Done!"
exit $EXIT_CODE
