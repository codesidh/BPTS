using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using WorkIntakeSystem.Infrastructure.Data;
using WorkIntakeSystem.Core.Interfaces;
using WorkIntakeSystem.Core.Services;
using WorkIntakeSystem.Infrastructure.Repositories;
using FluentValidation;
using AutoMapper;
using System.Reflection;
using WorkIntakeSystem.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/workintake-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Work Intake System API",
        Version = "v1",
        Description = "Enterprise work intake management system API"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database configuration
builder.Services.AddDbContext<WorkIntakeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("WorkIntakeSystem.Infrastructure")));

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtSecret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT secret not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "WorkIntakeSystem",
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"] ?? "WorkIntakeSystem",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Memory caching for authentication
builder.Services.AddMemoryCache();

// Redis caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Register repositories
builder.Services.AddScoped<IWorkRequestRepository, WorkRequestRepository>();
builder.Services.AddScoped<IPriorityRepository, PriorityRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<ISystemConfigurationRepository, SystemConfigurationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWorkflowEngine, WorkflowEngine>();

// Authentication services
builder.Services.AddScoped<IJwtAuthenticationService, JwtAuthenticationService>();

// API Gateway services
builder.Services.AddScoped<IApiGatewayService, ApiGatewayService>();
builder.Services.AddScoped<IRateLimitingService, RateLimitingService>();
builder.Services.AddScoped<IApiVersioningService, ApiVersioningService>();
builder.Services.AddScoped<IRequestTransformationService, RequestTransformationService>();

// Multi-tier caching services
builder.Services.AddScoped<IMultiTierCachingService, MultiTierCachingService>();
builder.Services.AddScoped<IDatabaseQueryCacheService, DatabaseQueryCacheService>();
builder.Services.AddScoped<IConfigurationCacheService, ConfigurationCacheService>();
builder.Services.AddScoped<IIISOutputCacheService, IISOutputCacheService>();

// Service Broker messaging services
builder.Services.AddScoped<IServiceBrokerService, ServiceBrokerService>();
builder.Services.AddSingleton<IMessageHandlerRegistry, MessageHandlerRegistry>();

// Background services
builder.Services.AddHostedService<ServiceBrokerHostedService>();

// Register services
builder.Services.AddScoped<IPriorityCalculationService, PriorityCalculationService>(sp =>
    new PriorityCalculationService(
        sp.GetRequiredService<IWorkRequestRepository>(),
        sp.GetRequiredService<IPriorityRepository>(),
        sp.GetRequiredService<IDepartmentRepository>(),
        sp.GetRequiredService<IConfigurationService>()
    )
);
builder.Services.AddScoped<IConfigurationService, ConfigurationService>(sp =>
    new ConfigurationService(
        sp.GetRequiredService<ISystemConfigurationRepository>(),
        sp.GetRequiredService<IConfiguration>()
    )
);

// Register analytics and integration services
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IExternalIntegrationService, ExternalIntegrationService>();
builder.Services.AddScoped<IProjectManagementIntegration, ProjectManagementIntegration>();
builder.Services.AddScoped<ICalendarIntegration, CalendarIntegration>();
builder.Services.AddScoped<INotificationIntegration, NotificationIntegration>();

// Register HttpClient for external integrations
builder.Services.AddHttpClient();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContext<WorkIntakeDbContext>()
    .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Work Intake System API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");

// Add API Gateway middleware before authentication
app.UseMiddleware<WorkIntakeSystem.Infrastructure.Middleware.ApiGatewayMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WorkIntakeDbContext>();
    try
    {
        context.Database.EnsureCreated();
        Log.Information("Database connection established successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to establish database connection");
    }
}

Log.Information("Work Intake System API starting up...");

app.Run();