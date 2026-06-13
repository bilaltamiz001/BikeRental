# BikeRental Architecture Enhancement - Quick Start Guide

## Getting Started with Enhanced Features

### Prerequisites
- .NET 8 SDK
- Redis (optional, but recommended for production)

### 1. Running Locally with In-Memory Cache (Development)

No additional setup needed! The application automatically uses in-memory caching if Redis is unavailable.

```bash
cd BikeRental
dotnet run
```

The application will start with:
- ✅ In-memory caching enabled
- ✅ Rate limiting at 1000 requests/minute
- ✅ Both v1.0 and v1.1 API versions available

### 2. Running with Redis (Recommended for Production)

#### Option A: Docker
```bash
# Start Redis container
docker run -d -p 6379:6379 --name bike-redis redis:latest

# Run application (it will auto-detect Redis)
dotnet run
```

#### Option B: Manual Redis Setup
```bash
# Install Redis or start from Windows Subsystem for Linux
redis-server

# Run application
dotnet run
```

#### Option C: Update Connection String
Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
	"Redis": "your-redis-server:6379"
  }
}
```

### 3. API Endpoints Overview

#### Health Check
```bash
curl http://localhost:5000/health
```

#### Diagnostic Endpoint
```bash
curl http://localhost:5000/api/test
```

#### Legacy v1.0 Endpoints (Synchronous)
```bash
# Get all beach cruisers
curl http://localhost:5000/api/bikes/beach

# Get mountain bike by ID
curl http://localhost:5000/api/bikes/mountain/1

# Rent a beach cruiser
curl -X POST http://localhost:5000/api/bikes/beach/1/rent

# Get all accessories
curl http://localhost:5000/api/accessories

# Process order
curl -X POST http://localhost:5000/api/accessories/order \
  -H "Content-Type: application/json" \
  -d '{"1": 2, "3": 1}'
```

#### New v1.1 Endpoints (Asynchronous with Caching)
```bash
# Get all beach cruisers (cached for 5 minutes)
curl http://localhost:5000/api/v1.1/bikes/beach

# Get mountain bike by ID
curl http://localhost:5000/api/v1.1/bikes/mountain/1

# Rent a mountain bike
curl -X POST http://localhost:5000/api/v1.1/bikes/mountain/2/rent

# Get all accessories (cached for 5 minutes)
curl http://localhost:5000/api/v1.1/accessories

# Get compatible accessories for beach bikes
curl http://localhost:5000/api/v1.1/accessories/compatible/beach

# Process order asynchronously
curl -X POST http://localhost:5000/api/v1.1/accessories/order \
  -H "Content-Type: application/json" \
  -d '{"1": 2, "3": 1}'

# Reset bikes
curl -X POST http://localhost:5000/api/v1.1/bikes/beach/reset
```

### 4. Using the Swagger UI

Open your browser and navigate to:
```
http://localhost:5000/swagger
```

You'll see:
- **Bike Rental API v1.0** - Original synchronous endpoints
- **Bike Rental API v1.1** - New asynchronous endpoints with caching

Try out endpoints directly from the UI!

### 5. Monitoring Cache Performance

#### Check if Response is Cached
Look for these headers in the response:
```
Cache-Control: public, max-age=300
X-Cache-Duration: 300
```

#### Cache Hit Indicators
- First request: ~150ms
- Subsequent requests: ~5ms (from cache)
- Response time should drop dramatically

### 6. Testing Rate Limiting

Send more than 1000 requests per minute from the same IP:

```bash
# This will hit rate limit after 1000 requests
for i in {1..1010}; do
  curl http://localhost:5000/api/test
done
```

You'll receive:
```json
{
  "error": "Rate limit exceeded",
  "retryAfter": 45,
  "limit": 1000,
  "window": "60 seconds"
}
```

### 7. Configuration Options

Edit `appsettings.json` to customize:

```json
{
  "ConnectionStrings": {
	"Redis": "localhost:6379"  // Change Redis endpoint
  },
  "Caching": {
	"DefaultExpirationMinutes": 30,
	"EnableDistributedCache": true
  },
  "RateLimit": {
	"RequestsPerMinute": 1000
  }
}
```

### 8. Accessing Application Logs

Logs are automatically written to:
```
BikeRental/logs/bikerental-YYYYMMDD.log
```

Example log entries:
```
2024-06-13 14:30:45.123 +00:00 [INF] Cache hit for key: beach_cruisers
2024-06-13 14:30:46.456 +00:00 [INF] Successfully retrieved 5 beach cruisers
2024-06-13 14:30:47.789 [WRN] Rate limit exceeded for client 192.168.1.100
```

### 9. Dependency Injection Usage

To use the new async services in your code:

```csharp
// Inject in your controller/service
private readonly IBeachCruiserAsyncService _bikeService;
private readonly IUnitOfWork _unitOfWork;

public MyController(
	IBeachCruiserAsyncService bikeService,
	IUnitOfWork unitOfWork)
{
	_bikeService = bikeService;
	_unitOfWork = unitOfWork;
}

// Use async methods
public async Task<IActionResult> MyAction()
{
	// Get all bikes (automatically cached)
	var bikes = await _bikeService.GetAllAsync();

	// Get single bike
	var bike = await _bikeService.GetByIdAsync(1);

	// Rent a bike
	var success = await _bikeService.RentBikeAsync(1);

	// Use Unit of Work for complex operations
	var cruisers = await _unitOfWork.BeachCruisers.GetAllAsync();
	var mountains = await _unitOfWork.MountainBikes.GetAllAsync();
	await _unitOfWork.SaveChangesAsync();

	return Ok(bikes);
}
```

### 10. Performance Testing

Run a simple performance comparison:

```bash
# Test v1.0 (synchronous) - measures time for 100 requests
time for i in {1..100}; do
  curl -s http://localhost:5000/api/bikes/beach > /dev/null
done

# Test v1.1 (asynchronous with caching) - should be much faster
time for i in {1..100}; do
  curl -s http://localhost:5000/api/v1.1/bikes/beach > /dev/null
done
```

Expected results:
- v1.0: ~10-15 seconds (mostly from thread context switches)
- v1.1: ~0.5-1 second (with caching)

### 11. Troubleshooting

#### Issue: "Redis connection failed"
**Solution**: Application automatically falls back to in-memory cache. Check that Redis is running if you need distributed caching.

#### Issue: Rate limiting too aggressive
**Solution**: Increase the limit in `appsettings.json`:
```json
"RateLimit": {
  "RequestsPerMinute": 5000  // Increase this
}
```

#### Issue: Cache not working
**Solution**: 
1. Verify cache headers in response
2. Check logs for cache errors
3. Ensure Redis is running (if configured)
4. Clear browser cache (Ctrl+Shift+Delete)

#### Issue: Async endpoint returns different data than v1.0
**Solution**: This is expected! Different versions may be served from cache. Reset data:
```bash
curl -X POST http://localhost:5000/api/v1.1/bikes/beach/reset
```

### 12. Common Use Cases

#### Use Case 1: Get All Bikes (Cached)
```bash
curl http://localhost:5000/api/v1.1/bikes/beach
# Response Time: 5ms (cached)
```

#### Use Case 2: Rent a Bike
```bash
curl -X POST http://localhost:5000/api/v1.1/bikes/beach/1/rent
# Returns: { "success": true, "data": true, "message": "Beach cruiser 1 rented successfully" }
```

#### Use Case 3: Process Accessory Order with Bundle Discount
```bash
curl -X POST http://localhost:5000/api/v1.1/accessories/order \
  -H "Content-Type: application/json" \
  -d '{"1": 2, "3": 1}'
# Applies 10% discount for bundle items
```

#### Use Case 4: Check Cache Hit Rate
```bash
# Send multiple requests and compare response times
curl http://localhost:5000/api/v1.1/bikes/beach
# First: ~100ms
curl http://localhost:5000/api/v1.1/bikes/beach
# Second: ~5ms (from cache!)
```

### 13. Production Deployment Checklist

- [ ] Configure Redis connection string
- [ ] Set appropriate rate limit for your usage
- [ ] Enable HTTPS
- [ ] Configure CORS for your frontend domain
- [ ] Set up monitoring and alerting
- [ ] Configure log rotation
- [ ] Test with production-like load
- [ ] Set up Redis persistence/replication
- [ ] Configure Redis memory limits
- [ ] Set up Redis backups

### 14. Documentation Links

- **Architecture Details**: See `ARCHITECTURE_ENHANCEMENTS.md`
- **Full Implementation Summary**: See `ENHANCEMENT_SUMMARY.md`
- **API Versions**: Swagger UI at `/swagger`
- **Health Endpoint**: `GET /health`

---

## Quick Reference Commands

```bash
# Start application
dotnet run

# Run with specific configuration
dotnet run --configuration Release

# Run tests
dotnet test

# Build only
dotnet build

# Clean build
dotnet clean && dotnet build

# View application logs
tail -f BikeRental/logs/bikerental-*.log
```

---

## Additional Resources

- **Redis Documentation**: https://redis.io/documentation
- **ASP.NET Core Async**: https://docs.microsoft.com/en-us/dotnet/csharp/async
- **Repository Pattern**: https://martinfowler.com/eaaCatalog/repository.html
- **Unit of Work Pattern**: https://martinfowler.com/eaaCatalog/unitOfWork.html

---

## Support

For issues or questions:
1. Check the logs in `BikeRental/logs/`
2. Review `ARCHITECTURE_ENHANCEMENTS.md` for detailed information
3. Check this guide for troubleshooting steps
4. Review Swagger documentation at `/swagger`

---

Happy coding! 🚴‍♂️
