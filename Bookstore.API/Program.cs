using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Bookstore.Data;
using Bookstore.Data.Interfaces;
using Bookstore.Data.Repositories;
using Bookstore.Business.Interfaces;
using Bookstore.Business.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Service Registrations
builder.Services.AddControllers(); // Controllers ko register kar rahe hain

// Database Configuration with Conditional Logging
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // SQL Server se connection string use kar rahe hain
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    // Development environment mein detailed SQL logs enable kar rahe hain
    if (builder.Environment.IsDevelopment())
    {
        options.LogTo(
            message => Console.WriteLine($"DB Query: {message}"), // Log messages ko format kar rahe hain
            LogLevel.Information // Log level ko define kar rahe hain (Information, Debug, etc.)
        )
        .EnableDetailedErrors(); // Detailed error messages show karne ke liye
    }
});

// Enhanced Validation Response Configuration
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true; // Default validation filter ko suppress kar rahe hain

    options.InvalidModelStateResponseFactory = context =>
    {
        // Model state errors ko extract kar rahe hain
        var errors = context.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .SelectMany(kvp => kvp.Value.Errors
                .Select(e => new
                {
                    Field = kvp.Key,
                    Message = !string.IsNullOrEmpty(e.ErrorMessage)
                        ? e.ErrorMessage
                        : "Invalid value format" // Default error message
                }))
            .ToList();

        // Response return kar rahe hain BadRequest ke sath error details
        return new BadRequestObjectResult(new
        {
            Status = "ValidationError",
            Errors = errors,
            Solution = "Check validation rules for each field"
        });
    };
});

// JWT Authentication (Fixed Claim Mapping)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Issuer validate kar rahe hain
            ValidateAudience = true, // Audience validate kar rahe hain
            ValidateLifetime = true, // Token ke expiry ko validate kar rahe hain
            ValidateIssuerSigningKey = true, // Issuer signing key ko validate kar rahe hain
            ValidIssuer = builder.Configuration["Jwt:Issuer"], // Valid issuer ko set kar rahe hain
            ValidAudience = builder.Configuration["Jwt:Audience"], // Valid audience ko set kar rahe hain
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])) // Signing key set kar rahe hain
        };
    });

// Dependency Injection
builder.Services.AddScoped<IUserRepository, UserRepository>(); // IUserRepository ko UserRepository se map kar rahe hain
builder.Services.AddScoped<IAdminRepository, AdminRepository>(); // IAdminRepository ko AdminRepository se map kar rahe hain
builder.Services.AddScoped<IUserAuthService, UserAuthService>(); // IUserAuthService ko UserAuthService se map kar rahe hain
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>(); // IAdminAuthService ko AdminAuthService se map kar rahe hain
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>(); // RefreshTokenRepository ko add kar rahe hain
builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>(); // PasswordResetRepository ko add kar rahe hain
builder.Services.AddScoped<IForgotPasswordService, ForgotPasswordService>(); // ForgotPasswordService ko add kar rahe hain
builder.Services.AddScoped<ITokenService, TokenService>(); // TokenService ko add kar rahe hain

// Swagger Configuration
builder.Services.AddEndpointsApiExplorer(); // Endpoints ko explore kar rahe hain
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Bookstore API", Version = "v1" }); // API documentation create kar rahe hain

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme", // JWT authorization ke liye description de rahe hain
        Name = "Authorization", // Header mein Authorization ke naam se pass hoga
        In = ParameterLocation.Header, // Header mein authorization pass karenge
        Type = SecuritySchemeType.Http, // HTTP scheme type use kar rahe hain
        Scheme = "bearer", // Bearer scheme set kar rahe hain
        BearerFormat = "JWT" // JWT format specify kar rahe hain
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer" // Bearer authorization ko required bana rahe hain
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Content Type Validation Middleware
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/swagger"), appBuilder =>
{
    appBuilder.Use(async (context, next) =>
    {
        // Agar request ka content type JSON nahi hai toh error dikhayenge
        if (!context.Request.HasJsonContentType())
        {
            context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
            await context.Response.WriteAsJsonAsync(new
            {
                Status = "Error",
                Message = "Request must be in JSON format", // JSON format ke alawa kuch nahi chalega
                SupportedTypes = new[] { "application/json" }
            });
            return;
        }
        await next();
    });
});

// Error Handling Middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        var error = context.Features.Get<IExceptionHandlerFeature>();

        // Exception handle karte waqt response mein error message bhej rahe hain
        await context.Response.WriteAsJsonAsync(new
        {
            StatusCode = context.Response.StatusCode,
            Message = "Request processing failed", // General error message
            Detailed = app.Environment.IsDevelopment() ? error?.Error.Message : null, // Development mein detailed error message dikhayenge
            Solutions = new[] {
                "Check input format", // Solution options
                "Verify required fields",
                "Review API documentation"
            }
        });
    });
});

// Swagger Configuration (Development Only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Swagger ko enable kar rahe hain
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bookstore API v1"); // Swagger UI ke endpoint ko set kar rahe hain
        c.RoutePrefix = "swagger"; // Route prefix ko 'swagger' set kar rahe hain
        c.DocumentTitle = "Bookstore API Documentation"; // Swagger document ka title set kar rahe hain
    });
}

// Pipeline Configuration
app.UseHttpsRedirection(); // HTTPS redirection ko enable kar rahe hain
app.UseRouting(); // Routing ko enable kar rahe hain
app.UseAuthentication(); // Authentication middleware ko use kar rahe hain
app.UseAuthorization(); // Authorization middleware ko use kar rahe hain
app.MapControllers(); // Controllers ko map kar rahe hain

app.Run(); // Application ko run kar rahe hain
