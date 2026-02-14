#!/bin/bash

# Script to build and run the IQA Management Hub for the first time
# This script handles all necessary steps to get the project running locally

set -e

# Determine the script's directory and project root
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$SCRIPT_DIR"
FRONTEND_DIR="$PROJECT_ROOT/src/frontend"
BACKEND_DIR="$PROJECT_ROOT/src/backend"
SERVICE_DIR="$PROJECT_ROOT/src/backend/ManagementHub.Service"

echo "======================================"
echo "IQA Management Hub - Local Setup"
echo "======================================"
echo ""

# Check prerequisites
echo "Checking prerequisites..."

# Check for Node.js
if ! command -v node &> /dev/null; then
    echo "Error: Node.js is not installed. Please install Node.js 18.x or later."
    exit 1
fi

# Check for Yarn
if ! command -v yarn &> /dev/null; then
    echo "Error: Yarn is not installed. Please install Yarn package manager."
    exit 1
fi

# Check for .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET SDK is not installed. Please install .NET SDK 8.0 or later."
    exit 1
fi

echo "✓ All prerequisites found"
echo "  - Node.js: $(node --version)"
echo "  - Yarn: $(yarn --version)"
echo "  - .NET: $(dotnet --version)"
echo ""

# Build Frontend
echo "======================================"
echo "Building Frontend"
echo "======================================"
echo ""

cd "$FRONTEND_DIR"

echo "Installing frontend dependencies..."
yarn install --immutable

echo ""
echo "Building frontend for development..."
yarn build:dev

echo ""
echo "✓ Frontend build completed"
echo ""

# Build Backend
echo "======================================"
echo "Building Backend"
echo "======================================"
echo ""

cd "$BACKEND_DIR"

echo "Restoring backend dependencies..."
dotnet restore

echo ""
echo "Building backend solution..."
dotnet build

echo ""
echo "✓ Backend build completed"
echo ""

# Run Application
echo "======================================"
echo "Starting Application"
echo "======================================"
echo ""

cd "$SERVICE_DIR"

echo "Starting the Management Hub service..."
echo "The application will be available at: http://localhost:5000"
echo ""
echo "Default test users (password: 'password'):"
echo "  - Referee: referee@example.com"
echo "  - NGB Admin: ngb_admin@example.com"
echo "  - IQA Admin: iqa_admin@example.com"
echo ""
echo "Press Ctrl+C to stop the server"
echo ""
echo "======================================"
echo ""

# Run the application
dotnet run
