#!/bin/bash

# Script to update the IQA Management Hub with latest changes from repository
# This script pulls the latest code and updates dependencies as needed

set -e

# Determine the script's directory and project root
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$SCRIPT_DIR"
FRONTEND_DIR="$PROJECT_ROOT/src/frontend"
BACKEND_DIR="$PROJECT_ROOT/src/backend"

echo "======================================"
echo "IQA Management Hub - Update"
echo "======================================"
echo ""

# Check if we're in a git repository
if [ ! -d "$PROJECT_ROOT/.git" ]; then
    echo "Error: Not a git repository. Please clone the repository first."
    exit 1
fi

cd "$PROJECT_ROOT"

# Check for uncommitted changes
if ! git diff-index --quiet HEAD -- 2>/dev/null; then
    echo "Warning: You have uncommitted changes in your working directory."
    echo "Please commit or stash your changes before updating."
    echo ""
    git status --short
    echo ""
    read -p "Do you want to continue anyway? (y/N) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "Update cancelled."
        exit 1
    fi
fi

# Get current branch
CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)
echo "Current branch: $CURRENT_BRANCH"
echo ""

# Pull latest changes
echo "Pulling latest changes from repository..."
git pull origin "$CURRENT_BRANCH"

echo ""
echo "✓ Repository updated"
echo ""

# Check if package.json changed
if git diff --name-only ORIG_HEAD HEAD 2>/dev/null | grep -q "src/frontend/package.json"; then
    echo "Frontend dependencies have changed, updating..."
    cd "$FRONTEND_DIR"
    yarn install --immutable
    echo "✓ Frontend dependencies updated"
    echo ""
fi

# Check if backend project files changed
if git diff --name-only ORIG_HEAD HEAD 2>/dev/null | grep -q "src/backend/.*\.csproj"; then
    echo "Backend dependencies have changed, updating..."
    cd "$BACKEND_DIR"
    dotnet restore
    echo "✓ Backend dependencies updated"
    echo ""
fi

echo "======================================"
echo "Update Complete!"
echo "======================================"
echo ""
echo "You can now rebuild and run the application with:"
echo "  ./start.sh"
echo ""
echo "Or build manually:"
echo "  Frontend: cd src/frontend && yarn build:dev"
echo "  Backend:  cd src/backend && dotnet build"
echo ""
