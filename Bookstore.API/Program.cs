using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using System.Text.Json.Serialization;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Bookstore.API.Authorization;
using Bookstore.Business.Interfaces;
using Bookstore.Business.Services;
using Bookstore.Data;
using Bookstore.Data.Interfaces;
using Bookstore.Data.Repositories;
using System.Security.Claims;
using Bookstore.API.Swagger;

// Initialize the web application builder
var builder = WebApplication.CreateBuilder(args);

// ==================== SERVICE REGISTRATIONS ====================

// Configure controllers with JSON serialization options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Prevent reference loops in object graphs
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // Omit null values from JSON responses
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// CORS Configuration - Allow all origins for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Database Configuration
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Use SQL Server with connection string from appsettings
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Bookstore.Data"));

    // Enable detailed logging in development environment
    if (builder.Environment.IsDevelopment())
    {
        options.LogTo(Console.WriteLine, LogLevel.Information)
               .EnableDetailedErrors();
    }
});

// Configure API behavior for validation errors
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    // Disable automatic model state validation
    options.SuppressModelStateInvalidFilter = true;

    // Custom response for invalid model state
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .SelectMany(kvp => kvp.Value.Errors
                .Select(e => new
                {
                    Field = kvp.Key,
                    Message = e.ErrorMessage ?? "Invalid format"
                }));

        return new BadRequestObjectResult(new
        {
            Status = "ValidationError",
            Errors = errors,
            Solution = "Check API docs"
        });
    };
});

// JWT Authentication Configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.FromMinutes(5),

            // Configure claim types for ASP.NET Core Identity integration
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    });

// Dependency Injection Registrations

// Data Access Layer (Repositories)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();

// ?? New Repositories for Address & Order Management
builder.Services.AddScoped<ICustomerAddressRepository, CustomerAddressRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Business Layer (Services)
builder.Services.AddScoped<IUserAuthService, UserAuthService>();
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IForgotPasswordService, ForgotPasswordService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();

// ?? New Services for Address & Order Management
builder.Services.AddScoped<ICustomerAddressService, CustomerAddressService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Swagger/OpenAPI Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Enable file upload support in Swagger
    c.OperationFilter<FileUploadOperationFilter>();

    // API documentation metadata
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Bookstore API",
        Version = "v1",
        Description = "Comprehensive API for managing books, users, admins, carts, wishlists, addresses, and orders"
    });

    // JWT Bearer authentication configuration for Swagger
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Auth",
        Description = "Enter JWT Bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// Build the application
var app = builder.Build();

// ================= MIDDLEWARE PIPELINE CONFIGURATION =================

// Conditional middleware for JSON content validation
app.UseWhen(context =>
    context.Request.Method is "POST" or "PUT" or "PATCH" &&
    // Exclude endpoints that don't require JSON bodies
    !context.Request.Path.StartsWithSegments("/api/Books/import") &&
    !context.Request.Path.StartsWithSegments("/api/Cart") &&
    !context.Request.Path.StartsWithSegments("/api/Wishlist"),
appBuilder =>
{
    appBuilder.Use(async (context, next) =>
    {
        // Validate content type for supported HTTP methods
        if (!context.Request.HasJsonContentType())
        {
            context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
            await context.Response.WriteAsJsonAsync(new
            {
                ErrorCode = "MEDIA-001",
                Message = "JSON format required"
            });
            return;
        }
        await next();
    });
});

// Global exception handling middleware
app.UseExceptionHandler(handler =>
{
    handler.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        var error = context.Features.Get<IExceptionHandlerFeature>();

        await context.Response.WriteAsJsonAsync(new
        {
            StatusCode = context.Response.StatusCode,
            Message = "Request failed",
            Debug = app.Environment.IsDevelopment() ? error?.Error.Message : null
        });
    });
});

// Swagger UI configuration (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bookstore API v1");
        c.EnableTryItOutByDefault(); // Enable interactive features
    });
}

// Core application middleware
app.UseHttpsRedirection();       // Enforce HTTPS
app.UseRouting();                // Enable endpoint routing
app.UseCors("AllowAll");         // Apply CORS policy
app.UseAuthentication();         // Enable authentication
app.UseAuthorization();          // Enable authorization
app.MapControllers();            // Map controller endpoints

// Start the application
app.Run();