#!/bin/bash
# Diagnostic script to test the BikeRental API endpoints

echo "=== BikeRental API Diagnostic Test ==="
echo ""

# Define the base URL (adjust if needed)
BASE_URL="http://localhost:5000"

# Try to detect the port from launchSettings.json if it exists
if [ -f "BikeRental/Properties/launchSettings.json" ]; then
	echo "Checking launchSettings.json for port information..."
fi

echo "Testing endpoint: GET $BASE_URL/api/bikes/beach"
curl -v "$BASE_URL/api/bikes/beach" 2>&1

echo ""
echo "Testing endpoint: GET $BASE_URL/api/bikes/mountain"
curl -v "$BASE_URL/api/bikes/mountain" 2>&1

echo ""
echo "Testing health endpoint: GET $BASE_URL/health"
curl -v "$BASE_URL/health" 2>&1
