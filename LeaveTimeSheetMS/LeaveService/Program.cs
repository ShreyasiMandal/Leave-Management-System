using LeaveService.Application.Consumers;
using LeaveService.Application.Interfaces;
using LeaveService.Application.Saga;
using LeaveService.Application.Services;
using LeaveService.Domain.Entities;
using LeaveService.Domain.Enums;
using LeaveService.Infastructure.Consumers;
using LeaveService.Infrastructure.Data;
using LeaveService.Infrastructure.Messaging;
using LeaveService.Infrastructure.Repositories;
using LeaveService.Middleware;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);



// ── 1. DATABASE ────────────────────────────────────────────────────────────
builder.Services.AddDbContext<LeaveDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("LeaveDB")));



// ── 2. JWT AUTHENTICATION ─────────────────────────────────────────────────
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



// ── 3. AUTHORIZATION ──────────────────────────────────────────────────────
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("ManagerOrAbove", p => p.RequireRole(
        UserRoles.Manager, UserRoles.HRAdmin, UserRoles.SystemAdmin));

    opt.AddPolicy("HROrAbove", p => p.RequireRole(
        UserRoles.HRAdmin, UserRoles.SystemAdmin));
});



// ── 4. MASS TRANSIT + SAGA ────────────────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    // Register Saga State Machine
    x.AddSagaStateMachine<LeaveApprovalStateMachine, LeaveApprovalSagaState>()
        .EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Pessimistic;

            r.AddDbContext<DbContext, LeaveDbContext>((provider, opt) =>
            {
                opt.UseSqlServer(
                    builder.Configuration.GetConnectionString("LeaveDB"));
            });
        });

    x.AddConsumer<ApproveLeaveBalanceConsumer>();
    x.AddConsumer<RestoreLeaveBalanceConsumer>();
    x.AddConsumer<UserCreatedLeaveConsumer>();

    // Configure RabbitMQ
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(
            builder.Configuration["RabbitMQ:Host"] ?? "localhost",
            "/",
            h =>
            {
                h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
                h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
            });

        // Saga endpoint
        cfg.ReceiveEndpoint("leave-approval-saga", e =>
        {
            e.ConfigureSaga<LeaveApprovalSagaState>(context);
        });

        cfg.ReceiveEndpoint("leave-balance-commands", e =>
        {
            e.ConfigureConsumer<ApproveLeaveBalanceConsumer>(context);
            e.ConfigureConsumer<RestoreLeaveBalanceConsumer>(context);
        });

        cfg.ReceiveEndpoint("ltma-leave-user-created", e =>
        {
            e.ConfigureConsumer<UserCreatedLeaveConsumer>(context);

        });
    });
});



// ── 5. DEPENDENCY INJECTION ───────────────────────────────────────────────
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
builder.Services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();
builder.Services.AddScoped<IHolidayRepository, HolidayRepository>();

builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
builder.Services.AddScoped<ILeaveEventPublisher, LeaveEventPublisher>();
builder.Services.AddScoped<ILeaveTypeService, LeaveTypeService>();
builder.Services.AddScoped<ILeaveBalanceService, LeaveBalanceService>();
builder.Services.AddScoped<IHolidayService, HolidayService>();

// ❌ REMOVED:
// ILeaveEventPublisher (because MassTransit handles messaging)


builder.Services.AddHttpClient();
// ── 6. CONTROLLERS + SWAGGER ──────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LTMA — Leave Service",
        Version = "v1"
    });

    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {{
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        Array.Empty<string>()
    }});
});



// ── 7. CORS ───────────────────────────────────────────────────────────────
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



// ── 8. AUTO MIGRATION ─────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LeaveDbContext>();
    db.Database.Migrate();
}



// ── 9. PIPELINE ───────────────────────────────────────────────────────────
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAngular");

app.UseCors();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();