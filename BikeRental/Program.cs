using BikeRental;
using BikeRental.Data;
using BikeRental.Data.UnitOfWork;
using BikeRental.Services;
using BikeRental.Services.Caching;
using BikeRental.Middleware;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Serilog;
using System.Reflection;

// Configure Serilog logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/bikerental-.log", 
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Starting BikeRental application");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog to the host builder
    builder.Host.UseSerilog();

    // Configure data folder path
    var dataFolder = Path.Combine(builder.Environment.ContentRootPath, "SampleData");

    // Add services to the container
    builder.Services.AddControllers();

    // Add memory cache for development
    builder.Services.AddMemoryCache();

    // Configure Redis caching (optional - fallback to memory cache)
    var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    try
    {
        var redis = ConnectionMultiplexer.Connect(redisConnectionString);
        builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
        builder.Services.AddSingleton<ICacheService, RedisCacheService>();
        Log.Information("Redis cache configured successfully");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Redis connection failed, falling back to in-memory cache");
        builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
    }

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Add health checks
    builder.Services.AddHealthChecks();

    // Add Swagger/OpenAPI documentation with versioning support
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Bike Rental API",
            Version = "v1.0",
            Description = "REST API for bike and accessory rental operations (Synchronous)",
            Contact = new OpenApiContact { Name = "Support" }
        });

        c.SwaggerDoc("v1.1", new OpenApiInfo
        {
            Title = "Bike Rental API",
            Version = "v1.1",
            Description = "REST API for bike and accessory rental operations (Asynchronous with Caching)",
            Contact = new OpenApiContact { Name = "Support" }
        });

        // Include XML comments from the generated documentation file
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    });

    // Register legacy repositories with interfaces
    var beachCruiserPath = Path.Combine(dataFolder, "beach_cruisers.xml");
    builder.Services.AddSingleton<IBeachCruiserRepository>(
        new BeachCruiserRepository(beachCruiserPath));

    var mountainBikePath = Path.Combine(dataFolder, "mountain_bikes.json");
    builder.Services.AddSingleton<IMountainBikeRepository>(
        new MountainBikeRepository(mountainBikePath));

    var accessoryPath = Path.Combine(dataFolder, "accessories.json");
    builder.Services.AddSingleton<IAccessoryRepository>(
        new AccessoryRepository(accessoryPath));

    // Register new async repositories with caching support
    builder.Services.AddSingleton<IBeachCruiserAsyncRepository>(sp =>
        new BeachCruiserAsyncRepository(
            beachCruiserPath,
            sp.GetRequiredService<ILogger<BeachCruiserAsyncRepository>>(),
            sp.GetRequiredService<ICacheService>()));

    builder.Services.AddSingleton<IMountainBikeAsyncRepository>(sp =>
        new MountainBikeAsyncRepository(
            mountainBikePath,
            sp.GetRequiredService<ILogger<MountainBikeAsyncRepository>>(),
            sp.GetRequiredService<ICacheService>()));

    builder.Services.AddSingleton<IAccessoryAsyncRepository>(sp =>
        new AccessoryAsyncRepository(
            accessoryPath,
            sp.GetRequiredService<ILogger<AccessoryAsyncRepository>>(),
            sp.GetRequiredService<ICacheService>()));

    // Register legacy services with logging
    builder.Services.AddSingleton<IBeachCruiserService>(sp =>
        new BeachCruiserService(
            sp.GetRequiredService<IBeachCruiserRepository>(),
            sp.GetRequiredService<ILogger<BeachCruiserService>>()));

    builder.Services.AddSingleton<IMountainBikeService>(sp =>
        new MountainBikeService(
            sp.GetRequiredService<IMountainBikeRepository>(),
            sp.GetRequiredService<ILogger<MountainBikeService>>()));

    builder.Services.AddSingleton<IAccessoryService>(sp =>
        new AccessoryService(
            sp.GetRequiredService<IAccessoryRepository>(),
            sp.GetRequiredService<ILogger<AccessoryService>>()));

    // Register new async services with improved scalability
    builder.Services.AddSingleton<IBeachCruiserAsyncService, BeachCruiserAsyncService>();
    builder.Services.AddSingleton<IMountainBikeAsyncService, MountainBikeAsyncService>();
    builder.Services.AddSingleton<IAccessoryAsyncService, AccessoryAsyncService>();

    // Register Unit of Work pattern for transaction coordination
    builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();

    // Register background monitor with factory
    builder.Services.AddSingleton<FleetMonitor>(_ => new FleetMonitor(dataFolder));

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bike Rental API v1.0 (Synchronous)");
            c.SwaggerEndpoint("/swagger/v1.1/swagger.json", "Bike Rental API v1.1 (Asynchronous)");
            c.RoutePrefix = "swagger";
        });
    }

    // Global exception handling middleware
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // Add rate limiting middleware for production readiness
    app.UseRateLimiting(requestsPerMinute: 1000);

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseCors("AllowAll");
    app.UseRouting();
    app.UseAuthorization();

    // Map health check endpoint
    app.MapHealthChecks("/health");

    // Diagnostic endpoint for testing
    app.MapGet("/api/test", () => new 
    { 
        message = "API is working", 
        timestamp = DateTime.UtcNow,
        apiVersions = new[] { "v1.0 (Synchronous)", "v1.1 (Asynchronous with Caching)" }
    });

    app.MapControllers();

    app.MapGet("/", context =>
    {
        context.Response.Redirect("/index.html");
        return Task.CompletedTask;
    });

    // Initialize data and start background monitor
    var monitor = app.Services.GetRequiredService<FleetMonitor>();
    monitor.Start();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.Information("Shutting down BikeRental application");
    await Log.CloseAndFlushAsync();
}

