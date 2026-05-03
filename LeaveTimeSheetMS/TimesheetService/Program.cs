using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using TimesheetService.Application.Interfaces;
using TimesheetService.Application.Services;
using TimesheetService.Domain.Enums;
using TimesheetService.Infrastructure.Data;
using TimesheetService.Infrastructure.Messaging;
using TimesheetService.Infrastructure.Repositories;
using TimesheetService.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── 1. DATABASE ────────────────────────────────────────────────────────────
builder.Services.AddDbContext<TimesheetDbContext>(opt =>
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("TimesheetDB")));

// ── 2. JWT ─────────────────────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

// ── 3. AUTHORIZATION POLICIES ──────────────────────────────────────────────
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("ManagerOrAbove", p => p.RequireRole(
        UserRoles.Manager, UserRoles.HRAdmin, UserRoles.SystemAdmin));
    opt.AddPolicy("HROrAbove", p => p.RequireRole(
        UserRoles.HRAdmin, UserRoles.SystemAdmin));
});

// ── 4. RABBITMQ ────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IConnection>(_ =>
    new ConnectionFactory
    {
        HostName = builder.Configuration["RabbitMQ:Host"] ?? "localhost",
        Port = int.Parse(builder.Configuration["RabbitMQ:Port"] ?? "5672"),
        UserName = builder.Configuration["RabbitMQ:Username"] ?? "guest",
        Password = builder.Configuration["RabbitMQ:Password"] ?? "guest"
    }.CreateConnection());

// ── 5. DEPENDENCY INJECTION ────────────────────────────────────────────────
builder.Services.AddScoped<ITimesheetRepository, TimesheetRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITimesheetService,
    TimesheetService.Application.Services.TimesheetService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITimesheetEventPublisher, TimesheetEventPublisher>();

// ── 6. CONTROLLERS + SWAGGER ───────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LTMA — Timesheet Service",
        Version = "v1",
        Description = "Timesheet Entry, Weekly Submission, Approval"
    });
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your_jwt_token}"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {{
        new OpenApiSecurityScheme { Reference = new OpenApiReference
            { Type = ReferenceType.SecurityScheme, Id = "Bearer" }},
        Array.Empty<string>()
    }});
});

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


var app = builder.Build();

// ── 7. AUTO MIGRATE ────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TimesheetDbContext>();
    db.Database.Migrate();
}

// ── 8. PIPELINE ────────────────────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAngular");
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();