# API Request/Response Examples

## Working API Flow

### 1. Get All Beach Cruisers

**Request:**
```
GET http://localhost:56988/api/bikes/beach HTTP/1.1
Host: localhost:56988
Accept: application/json
```

**Successful Response (200 OK):**
```json
{
  "success": true,
  "message": "Beach cruisers retrieved successfully",
  "data": [
	{
	  "bike_id": 1,
	  "bike_name": "Sunset Drifter",
	  "color": "Coral",
	  "frame_size": "Medium",
	  "price_per_day": 14.99,
	  "available": true,
	  "description": "Smooth and laid-back, perfect for cruising the boardwalk at golden hour."
	},
	{
	  "bike_id": 2,
	  "bike_name": "Ocean Breeze",
	  "color": "Teal",
	  "frame_size": "Large",
	  "price_per_day": 16.99,
	  "available": true,
	  "description": "Wide handlebars and a cushy seat for the most relaxed ride on the coast."
	},
	// ... 4 more bikes
  ],
  "errors": null
}
```

### 2. Get All Mountain Bikes

**Request:**
```
GET http://localhost:56988/api/bikes/mountain HTTP/1.1
Host: localhost:56988
Accept: application/json
```

**Successful Response (200 OK):**
```json
{
  "success": true,
  "message": "Mountain bikes retrieved successfully",
  "data": [
	{
	  "bikeID": 101,
	  "modelName": "TrailBlazer X9",
	  "brand": "RideTech",
	  "gearCount": 21,
	  "suspensionType": "Full",
	  "frameMaterial": "Aluminum",
	  "dailyRate": 29.99,
	  "isAvailable": true,
	  "terrain": "All Mountain",
	  "weightKg": 13.5
	},
	{
	  "bikeID": 102,
	  "modelName": "Summit Shredder",
	  "brand": "PeakRider",
	  "gearCount": 27,
	  "suspensionType": "Hardtail",
	  "frameMaterial": "Carbon Fiber",
	  "dailyRate": 24.99,
	  "isAvailable": true,
	  "terrain": "Cross Country",
	  "weightKg": 11.2
	},
	// ... 4 more bikes
  ],
  "errors": null
}
```

### 3. Rent a Beach Cruiser

**Request:**
```
POST http://localhost:56988/api/bikes/beach/1/rent HTTP/1.1
Host: localhost:56988
Content-Type: application/json
Content-Length: 0
```

**Successful Response (200 OK):**
```json
{
  "success": true,
  "message": "Beach cruiser #1 rented successfully",
  "data": null,
  "errors": null
}
```

**Error Response - Already Rented (409 Conflict):**
```json
{
  "success": false,
  "message": "Beach cruiser 1 is not available for rental",
  "data": null,
  "errors": null
}
```

### 4. Rent a Mountain Bike

**Request:**
```
POST http://localhost:56988/api/bikes/mountain/101/rent HTTP/1.1
Host: localhost:56988
Content-Type: application/json
Content-Length: 0
```

**Successful Response (200 OK):**
```json
{
  "success": true,
  "message": "Mountain bike #101 rented successfully",
  "data": null,
  "errors": null
}
```

## Possible Error Responses

### 404 Not Found
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "The HTTP resource that matches the request URI was not found."
}
```

### 500 Internal Server Error
```json
{
  "success": false,
  "message": "An internal server error occurred.",
  "data": null,
  "errors": ["Error message from exception"]
}
```

### 409 Conflict (Resource Already Rented)
```json
{
  "success": false,
  "message": "Beach cruiser 1 is not available for rental",
  "data": null,
  "errors": null
}
```

## JavaScript/jQuery Examples

### Fetch Beach Cruisers
```javascript
$.getJSON('/api/bikes/beach', function(response) {
	if (response.success && response.data) {
		console.log('Bikes:', response.data);
		// response.data is an array of bike objects
	}
}).fail(function(jqXHR) {
	console.error('Failed to load bikes');
	console.error('Status:', jqXHR.status);
	console.error('Response:', jqXHR.responseJSON);
});
```

### Rent a Beach Cruiser
```javascript
$.ajax({
	url: '/api/bikes/beach/1/rent',
	type: 'POST',
	contentType: 'application/json',
	success: function(response) {
		if (response.success) {
			console.log('Rental successful:', response.message);
		}
	},
	error: function(jqXHR) {
		console.error('Rental failed:', jqXHR.status);
		if (jqXHR.responseJSON) {
			console.error('Message:', jqXHR.responseJSON.message);
		}
	}
});
```

### Using Fetch API
```javascript
// Get all beach cruisers
fetch('/api/bikes/beach')
	.then(response => {
		if (!response.ok) throw new Error(`HTTP ${response.status}`);
		return response.json();
	})
	.then(data => {
		console.log('Success:', data);
		console.log('Bikes:', data.data);  // This is the array
	})
	.catch(error => console.error('Error:', error));
```

## Response Format Notes

1. **All responses use PascalCase in C# but camelCase in JSON**
   - C# property: `Success` → JSON key: `"success"`
   - C# property: `Message` → JSON key: `"message"`
   - C# property: `Data` → JSON key: `"data"`

2. **Data field contains the actual bikes array**
   - Direct API access: `response.data` contains the array
   - Individual bike properties use snake_case for beach cruisers
   - Individual bike properties use camelCase for mountain bikes

3. **Beach Cruiser Properties (XML source):**
   - `bike_id`, `bike_name`, `color`, `frame_size`, `price_per_day`, `available`, `description`

4. **Mountain Bike Properties (JSON source):**
   - `BikeID`, `ModelName`, `Brand`, `GearCount`, `SuspensionType`, `FrameMaterial`, `DailyRate`, `IsAvailable`, `Terrain`, `WeightKg`

## Testing with curl

```bash
# Get beach cruisers
curl -X GET "http://localhost:56988/api/bikes/beach" -H "accept: application/json"

# Get mountain bikes
curl -X GET "http://localhost:56988/api/bikes/mountain" -H "accept: application/json"

# Rent a beach cruiser
curl -X POST "http://localhost:56988/api/bikes/beach/1/rent" \
  -H "accept: application/json" \
  -H "Content-Type: application/json" \
  -d ""

# Rent a mountain bike
curl -X POST "http://localhost:56988/api/bikes/mountain/101/rent" \
  -H "accept: application/json" \
  -H "Content-Type: application/json" \
  -d ""
```

## Testing with Swagger UI

1. Navigate to: `http://localhost:56988/swagger`
2. Expand the "bikes" section
3. Click on the endpoint you want to test
4. Click "Try it out"
5. Click "Execute"
6. Check the response

## Common Issues & Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| 404 Not Found | API endpoint doesn't exist | Check URL spelling, port number |
| 500 Internal Server Error | Backend error | Check Visual Studio output window for stack trace |
| Empty data array | Repository loading issue | Check sample data files exist and are readable |
| CORS error | Cross-origin request blocked | Should be allowed by default, check browser console |
| Timeout | Server not responding | Check if app is running, restart if needed |
