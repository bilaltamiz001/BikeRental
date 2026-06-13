# BikeRental Sample Data Loading - Troubleshooting Guide

## Issue
Beach Cruisers and Mountain Bikes pages show: **"Failed to load bikes. The server may be giving you the silent treatment."**

## Root Causes & Solutions

### 1. **Application Not Running or Port Incorrect**

**Check if the app is running:**
- Look in Visual Studio for a browser window opening
- Check if you see console output in the Debug window
- The app should be accessible at: `https://localhost:56987` or `http://localhost:56988`

**Solution:**
```
1. Stop the application (if running)
2. Click "Start" or press F5 in Visual Studio
3. Wait for the browser to open
4. Navigate to http://localhost:56988 (the HTTP port, not HTTPS)
```

### 2. **Check Browser Console for Detailed Errors**

**Steps:**
1. Open the Beach Cruisers page
2. Press `F12` to open Developer Tools
3. Click on the "Console" tab
4. Look for error messages with details about what went wrong
5. The updated code now logs the actual error code and response

**Common Errors:**
- `Error: 404` - API endpoint not found
- `Error: 403` - CORS issue or forbidden
- `Error: 500` - Server error (check the backend logs)

### 3. **Verify API Endpoints are Working**

**Test the diagnostic endpoint:**
- Open browser and go to: `http://localhost:56988/api/test`
- You should see: `{"message":"API is working","timestamp":"2026-..."}`
- If this fails with 404, the API routing isn't working

**Test the data endpoints directly:**
- Go to: `http://localhost:56988/api/bikes/beach`
- You should see JSON data with the beach cruiser bikes
- Go to: `http://localhost:56988/api/bikes/mountain`
- You should see JSON data with the mountain bikes

### 4. **Check Sample Data Files Exist**

**Verify the files are in the correct location:**
```
BikeRental\SampleData\beach_cruisers.xml
BikeRental\SampleData\mountain_bikes.json
```

**If files are missing:**
1. Create the XML file at `BikeRental\SampleData\beach_cruisers.xml`
2. Create the JSON file at `BikeRental\SampleData\mountain_bikes.json`
3. Make sure they have the proper content (see Sample Data section below)

### 5. **Check Visual Studio Debug Output**

**Look for errors in the Debug Window:**
1. In Visual Studio, go to View > Output (or press Ctrl+Alt+O)
2. Select "Debug" from the dropdown
3. Look for error messages about:
   - File not found errors
   - Serialization errors
   - Exception stack traces

**If you see file not found errors:**
- The data folder path might be wrong
- The program is looking for files in: `{ApplicationRoot}/SampleData/`
- Verify the files are actually in that folder

### 6. **CORS Configuration Issue**

**If the API endpoint works in direct browser access but fails in AJAX:**
- The CORS policy might be too restrictive
- Check that `AllowAll` CORS policy is enabled in Program.cs
- The current config allows any origin, method, and header

### 7. **Check Application Startup Logs**

**In Visual Studio Debug Output, look for:**
```
Starting BikeRental application
Fetching all beach cruisers
Successfully retrieved 6 beach cruisers
```

**If you see errors like:**
```
Failed to initialize beach cruiser service
Beach cruiser data file not found: C:\...\SampleData\beach_cruisers.xml
```

This means:
- The file path is wrong
- The file doesn't exist in that location
- File permissions issue

## Sample Data Files

### beach_cruisers.xml
Should contain 6 beach cruiser bikes in this format:
```xml
<?xml version="1.0" encoding="utf-8"?>
<beach_cruisers>
  <bike>
	<bike_id>1</bike_id>
	<bike_name>Sunset Drifter</bike_name>
	<color>Coral</color>
	<frame_size>Medium</frame_size>
	<price_per_day>14.99</price_per_day>
	<available>true</available>
	<description>Smooth and laid-back, perfect for cruising the boardwalk.</description>
  </bike>
  <!-- ... 5 more bikes ... -->
</beach_cruisers>
```

### mountain_bikes.json
Should be a JSON array with 6 mountain bikes:
```json
[
  {
	"BikeID": 101,
	"ModelName": "TrailBlazer X9",
	"Brand": "RideTech",
	"GearCount": 21,
	"SuspensionType": "Full",
	"FrameMaterial": "Aluminum",
	"DailyRate": 29.99,
	"IsAvailable": true,
	"Terrain": "All Mountain",
	"WeightKg": 13.5
  },
  // ... 5 more bikes ...
]
```

## Step-by-Step Debugging

### Step 1: Verify Files Exist
```powershell
cd C:\Source\BikeRentalWeb_dotnet48\
Get-ChildItem -Path BikeRental\SampleData\
```
You should see:
- `beach_cruisers.xml`
- `mountain_bikes.json`

### Step 2: Build and Run
```
1. In Visual Studio, click Build > Rebuild Solution
2. Wait for build to complete
3. Press F5 to start debugging
4. Wait for browser to open
```

### Step 3: Test API Directly
```
1. In the browser, paste: http://localhost:56988/api/test
2. You should see a JSON response
3. If you see 404, the API route isn't working
```

### Step 4: Test Data Endpoints
```
1. In the browser, paste: http://localhost:56988/api/bikes/beach
2. If this works, you'll see all beach cruiser data
3. Repeat for: http://localhost:56988/api/bikes/mountain
```

### Step 5: Check Browser Console
```
1. Open the Beach Cruisers page: http://localhost:56988/beach-cruisers.html
2. Press F12 to open DevTools
3. Click Console tab
4. You'll see detailed error messages showing what failed
5. Note the HTTP status code (404, 500, etc.)
```

## If All Else Fails

**Restart everything:**
```powershell
1. Close Visual Studio
2. Close all browser windows
3. Open Visual Studio
4. Click File > Open Folder (or Open Project)
5. Navigate to C:\Source\BikeRentalWeb_dotnet48\
6. Wait for it to load
7. Press F5 to start
```

**Check file permissions:**
```powershell
# Make sure you have read access to the SampleData folder
Get-Acl -Path "C:\Source\BikeRentalWeb_dotnet48\BikeRental\SampleData\" | Format-List
```

## Quick Health Check Commands

**From browser console (F12 > Console tab), paste:**
```javascript
// Check if jQuery is loaded
console.log(typeof jQuery); // Should print "function"

// Try to fetch beach bikes
fetch('/api/bikes/beach')
  .then(r => r.json())
  .then(d => console.log('Success:', d))
  .catch(e => console.error('Error:', e));
```

## Expected Behavior When Working

1. ✅ Open http://localhost:56988/index.html
2. ✅ Click "Beach Cruisers" button
3. ✅ Page loads and displays 6 beach cruiser cards
4. ✅ Each card shows: name, color, frame size, price, availability, description
5. ✅ 4 bikes show "Available" (green)
6. ✅ 2 bikes show "Rented Out" (red)

## Support Information

- **Application Root:** `C:\Source\BikeRentalWeb_dotnet48\`
- **Data Folder:** `C:\Source\BikeRentalWeb_dotnet48\BikeRental\SampleData\`
- **HTTP Port:** 56988
- **HTTPS Port:** 56987 (browser may warn about self-signed cert)
- **API Base:** `/api/bikes/`
- **Swagger UI:** `http://localhost:56988/swagger`
