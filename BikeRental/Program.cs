using BikeRental;
using BikeRental.Data;
using BikeRental.Services;
using BikeRental.Middleware;
using Microsoft.OpenApi.Models;
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

    // Add Swagger/OpenAPI documentation
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Bike Rental API",
            Version = "v1",
            Description = "REST API for bike and accessory rental operations",
            Contact = new OpenApiContact { Name = "Support" }
        });

        // Include XML comments from the generated documentation file
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    });

    // Register repositories with interfaces
    var beachCruiserPath = Path.Combine(dataFolder, "beach_cruisers.xml");
    builder.Services.AddSingleton<IBeachCruiserRepository>(
        new BeachCruiserRepository(beachCruiserPath));

    var mountainBikePath = Path.Combine(dataFolder, "mountain_bikes.json");
    builder.Services.AddSingleton<IMountainBikeRepository>(
        new MountainBikeRepository(mountainBikePath));

    var accessoryPath = Path.Combine(dataFolder, "accessories.json");
    builder.Services.AddSingleton<IAccessoryRepository>(
        new AccessoryRepository(accessoryPath));

    // Register services with logging
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
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bike Rental API v1");
            c.RoutePrefix = "swagger";
        });
    }

    // Global exception handling middleware
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseCors("AllowAll");
    app.UseRouting();
    app.UseAuthorization();

    // Map health check endpoint
    app.MapHealthChecks("/health");

    // Diagnostic endpoint for testing
    app.MapGet("/api/test", () => new { message = "API is working", timestamp = DateTime.UtcNow });

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

