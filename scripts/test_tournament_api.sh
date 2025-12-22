#!/usr/bin/env bash
# Integration test script for Tournament API endpoints
# Tests: sign in, create tournament, list tournaments, update tournament, get by ID

set -e

BASE_URL="${BASE_URL:-http://localhost:5000}"
echo "Testing Tournament API at: $BASE_URL"

# Step 1: Sign in to get bearer token
echo "Step 1: Authenticating..."
LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email": "referee@example.com", "password": "password"}')

TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.token')
if [ "$TOKEN" == "null" ] || [ -z "$TOKEN" ]; then
  echo "❌ Authentication failed. Response: $LOGIN_RESPONSE"
  exit 1
fi
echo "✓ Authentication successful"

# Step 2: Create a tournament with mock data
echo ""
echo "Step 2: Creating tournament..."
CREATE_RESPONSE=$(curl -s -X POST "$BASE_URL/api/v2/tournaments" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Test Tournament",
    "description": "Integration test tournament",
    "startDate": "'$(date -d '+30 days' +%Y-%m-%d)'",
    "endDate": "'$(date -d '+32 days' +%Y-%m-%d)'",
    "tournamentType": "club",
    "location": "Test City",
    "organizers": "Test Organizers",
    "isPrivate": false
  }')

TOURNAMENT_ID=$(echo "$CREATE_RESPONSE" | jq -r '.id')
if [ "$TOURNAMENT_ID" == "null" ] || [ -z "$TOURNAMENT_ID" ]; then
  echo "❌ Tournament creation failed. Response: $CREATE_RESPONSE"
  exit 1
fi
if [[ ! "$TOURNAMENT_ID" =~ ^TR_ ]]; then
  echo "❌ Tournament ID does not have TR_ prefix: $TOURNAMENT_ID"
  exit 1
fi
echo "✓ Tournament created with ID: $TOURNAMENT_ID"

# Step 3: Get tournaments and check the tournament is there
echo ""
echo "Step 3: Listing tournaments..."
LIST_RESPONSE=$(curl -s -X GET "$BASE_URL/api/v2/tournaments" \
  -H "Authorization: Bearer $TOKEN")

TOURNAMENT_COUNT=$(echo "$LIST_RESPONSE" | jq '. | length')
FOUND=$(echo "$LIST_RESPONSE" | jq --arg NAME "Test Tournament" '.[] | select(.name == $NAME) | .id')

if [ -z "$FOUND" ]; then
  echo "❌ Created tournament not found in list"
  exit 1
fi
echo "✓ Tournament found in list ($TOURNAMENT_COUNT total tournaments)"

# Step 4: Edit the tournament
echo ""
echo "Step 4: Updating tournament..."
UPDATE_RESPONSE=$(curl -s -X PUT "$BASE_URL/api/v2/tournaments/$TOURNAMENT_ID" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Updated Test Tournament",
    "description": "Updated description",
    "startDate": "'$(date -d '+30 days' +%Y-%m-%d)'",
    "endDate": "'$(date -d '+32 days' +%Y-%m-%d)'",
    "tournamentType": "national",
    "location": "Updated City",
    "organizers": "Updated Organizers",
    "isPrivate": false
  }')

if echo "$UPDATE_RESPONSE" | jq -e '.id' > /dev/null 2>&1; then
  echo "✓ Tournament updated successfully"
else
  echo "❌ Tournament update failed. Response: $UPDATE_RESPONSE"
  exit 1
fi

# Step 5: Get tournament by ID and check data was updated
echo ""
echo "Step 5: Getting tournament by ID..."
GET_RESPONSE=$(curl -s -X GET "$BASE_URL/api/v2/tournaments/$TOURNAMENT_ID" \
  -H "Authorization: Bearer $TOKEN")

NAME=$(echo "$GET_RESPONSE" | jq -r '.name')
DESCRIPTION=$(echo "$GET_RESPONSE" | jq -r '.description')
TYPE=$(echo "$GET_RESPONSE" | jq -r '.tournamentType')
LOCATION=$(echo "$GET_RESPONSE" | jq -r '.location')
IS_INVOLVED=$(echo "$GET_RESPONSE" | jq -r '.isCurrentUserInvolved')

if [ "$NAME" != "Updated Test Tournament" ]; then
  echo "❌ Name not updated. Expected 'Updated Test Tournament', got '$NAME'"
  exit 1
fi

if [ "$DESCRIPTION" != "Updated description" ]; then
  echo "❌ Description not updated. Expected 'Updated description', got '$DESCRIPTION'"
  exit 1
fi

if [ "$TYPE" != "national" ]; then
  echo "❌ Type not updated. Expected 'national', got '$TYPE'"
  exit 1
fi

if [ "$LOCATION" != "Updated City" ]; then
  echo "❌ Location not updated. Expected 'Updated City', got '$LOCATION'"
  exit 1
fi

if [ "$IS_INVOLVED" != "true" ]; then
  echo "❌ IsCurrentUserInvolved should be true for tournament creator, got '$IS_INVOLVED'"
  exit 1
fi

echo "✓ Tournament data verified:"
echo "  - Name: $NAME"
echo "  - Description: $DESCRIPTION"
echo "  - Type: $TYPE"
echo "  - Location: $LOCATION"
echo "  - IsCurrentUserInvolved: $IS_INVOLVED"

echo ""
echo "=============================="
echo "✅ All tests passed successfully!"
echo "=============================="
