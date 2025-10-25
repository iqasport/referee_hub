#!/bin/bash
# Frontend Dev Server Validation Script
# This script validates that the webpack dev server with HMR is working correctly

set -e

echo "=== Frontend Dev Server Validation ==="
echo ""

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

cd "$(dirname "$0")/../src/frontend"

echo "Step 1: Checking dependencies..."
if [ ! -d "node_modules" ]; then
  echo -e "${YELLOW}Installing dependencies...${NC}"
  yarn install --immutable
else
  echo -e "${GREEN}✓ Dependencies already installed${NC}"
fi

echo ""
echo "Step 2: Building styles and images (first time setup)..."
yarn styles && yarn images:copy
echo -e "${GREEN}✓ Styles and images built${NC}"

echo ""
echo "Step 3: Starting webpack dev server..."
echo -e "${YELLOW}The dev server will start and run in the foreground.${NC}"
echo -e "${YELLOW}Press Ctrl+C to stop it.${NC}"
echo ""
echo "Expected output:"
echo "  - Initial compilation: ~10-15 seconds"
echo "  - Files written to dist/ directory"
echo "  - Server running at http://localhost:8080/"
echo ""
echo "To test HMR:"
echo "  1. In another terminal, modify a .tsx file in app/"
echo "  2. Watch for automatic recompilation (should take ~1-5 seconds)"
echo "  3. Check that *.hot-update.js files are created in dist/"
echo ""

yarn start:dev
