using ApiGateway.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ── 1. OCELOT CONFIG ───────────────────────────────────────────────────────
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// ── 2. JWT ─────────────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing from appsettings.json");
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer is missing from appsettings.json");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience is missing from appsettings.json");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                ctx.Response.Headers.Append("Token-Error", ctx.Exception.Message);
                return Task.CompletedTask;
            },
            OnChallenge = ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode = 401;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync(
                    "{\"message\":\"Unauthorized. Valid JWT required.\"}");
            },
            OnForbidden = ctx =>
            {
                ctx.Response.StatusCode = 403;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync(
                    "{\"message\":\"Forbidden. No permission.\"}");
            }
        };
    });

// ── 3. CORS ────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy
            .WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});


// ── 4. CONTROLLERS + SWAGGER ───────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LTMA API Gateway",
        Version = "v1",
        Description = "Central gateway for all LTMA microservices."
    });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ── 5. OCELOT ──────────────────────────────────────────────────────────────
builder.Services.AddOcelot();

// ═══════════════════════════════════════════════════════════════════════════
var app = builder.Build();
// ═══════════════════════════════════════════════════════════════════════════

app.UseCors("AllowAngular");
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "LTMA Gateway v1");
    c.RoutePrefix = "swagger";
});

// ── CRITICAL: These 3 lines handle /health and / via controllers ───────────
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ── Controllers registered as endpoints BEFORE Ocelot middleware ───────────
#pragma warning disable ASP0014
app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/health", async context =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("""
        {
            "status": "Gateway Running",
            "port": 5000,
            "services": [
                { "name": "AuthService",         "port": 5001, "swagger": "http://localhost:5001/swagger" },
                { "name": "EmployeeService",     "port": 5002, "swagger": "http://localhost:5002/swagger" },
                { "name": "LeaveService",        "port": 5003, "swagger": "http://localhost:5003/swagger" },
                { "name": "TimesheetService",    "port": 5004, "swagger": "http://localhost:5004/swagger" },
                { "name": "NotificationService", "port": 5005, "swagger": "http://localhost:5005/swagger" },
                { "name": "ReportService",       "port": 5006, "swagger": "http://localhost:5006/swagger" }
            ]
        }
        """);
    });

    endpoints.MapControllers();
});
#pragma warning restore ASP0014

// ── Ocelot LAST ────────────────────────────────────────────────────────────
await app.UseOcelot();

app.Run();