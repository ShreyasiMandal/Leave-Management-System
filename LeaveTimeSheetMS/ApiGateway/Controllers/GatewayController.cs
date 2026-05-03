using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("[controller]")]
public class GatewayController : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        var html = @"<!DOCTYPE html>
<html lang='en'>
<head>
<meta charset='UTF-8'/>
<meta name='viewport' content='width=device-width, initial-scale=1.0'/>
<title>LTMA API Gateway</title>
<style>
  * { margin:0; padding:0; box-sizing:border-box; }
  body {
    font-family: Arial, sans-serif;
    background: linear-gradient(135deg,#1F4E78 0%,#2E75B6 100%);
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 20px;
  }
  .container {
    background: white;
    border-radius: 16px;
    box-shadow: 0 20px 60px rgba(0,0,0,0.3);
    width: 100%;
    max-width: 780px;
    overflow: hidden;
  }
  .header {
    background: #1F4E78;
    color: white;
    padding: 28px 32px;
    text-align: center;
  }
  .header h1 { font-size: 2.2rem; font-weight: 700; }
  .header p  { opacity: 0.8; margin-top: 6px; font-size: 0.95rem; }
  .header .badge {
    display: inline-block;
    background: #27ae60;
    color: white;
    font-size: 0.75rem;
    padding: 3px 10px;
    border-radius: 20px;
    margin-top: 8px;
    font-weight: 600;
  }
  .body { padding: 28px 32px; }
  .subtitle {
    color: #555;
    font-size: 0.9rem;
    margin-bottom: 24px;
    padding-bottom: 14px;
    border-bottom: 1px solid #eee;
  }
  .service-card {
    display: flex;
    align-items: center;
    justify-content: space-between;
    background: #f8f9fa;
    border: 1px solid #e8e8e8;
    border-radius: 10px;
    padding: 16px 20px;
    margin-bottom: 12px;
    transition: all 0.2s;
    border-left: 4px solid #ccc;
  }
  .service-card:hover {
    border-color: #2E75B6;
    background: #f0f6ff;
    transform: translateX(3px);
  }
  .service-card.auth     { border-left-color: #8e44ad; }
  .service-card.employee { border-left-color: #2980b9; }
  .service-card.leave    { border-left-color: #27ae60; }
  .service-card.timesheet{ border-left-color: #e67e22; }
  .service-card.notif    { border-left-color: #e74c3c; }
  .service-card.report   { border-left-color: #16a085; }
  .svc-info { flex: 1; }
  .svc-name {
    font-weight: 700;
    font-size: 1rem;
    color: #222;
    margin-bottom: 3px;
  }
  .svc-desc { font-size: 0.8rem; color: #777; }
  .svc-port {
    font-size: 0.75rem;
    background: #e8f0fb;
    color: #1F4E78;
    padding: 2px 8px;
    border-radius: 4px;
    font-weight: 600;
    margin-right: 12px;
  }
  .btn-swagger {
    background: #1F4E78;
    color: white;
    padding: 7px 16px;
    border-radius: 6px;
    text-decoration: none;
    font-size: 0.82rem;
    font-weight: 600;
    white-space: nowrap;
    transition: background 0.2s;
  }
  .btn-swagger:hover { background: #2E75B6; }
  .info-box {
    background: #fff8e1;
    border: 1px solid #ffe082;
    border-radius: 8px;
    padding: 14px 18px;
    margin-top: 20px;
    font-size: 0.84rem;
    color: #7a5800;
    line-height: 1.6;
  }
  .footer {
    background: #f8f9fa;
    border-top: 1px solid #eee;
    padding: 14px 32px;
    text-align: center;
    font-size: 0.78rem;
    color: #aaa;
  }
</style>
</head>
<body>
<div class='container'>
  <div class='header'>
    <h1>LTMA API Gateway</h1>
    <p>Leave & Timesheet Management System</p>
    <span class='badge'>✅ Gateway Running on Port 5000</span>
  </div>
  <div class='body'>
    <p class='subtitle'>
      Select a microservice to open its Swagger documentation.
      All API calls from Angular go through this gateway.
    </p>

    <div class='service-card auth'>
      <div class='svc-info'>
        <div class='svc-name'>🔐 Auth Service</div>
        <div class='svc-desc'>Login, Register, JWT, OTP, Email, Roles</div>
      </div>
      <span class='svc-port'>:5001</span>
      <a class='btn-swagger' href='http://localhost:5001/swagger'
         target='_blank'>Open Swagger</a>
    </div>

    <div class='service-card employee'>
      <div class='svc-info'>
        <div class='svc-name'>👥 Employee Service</div>
        <div class='svc-desc'>Profiles, Departments, Hierarchy, Team</div>
      </div>
      <span class='svc-port'>:5002</span>
      <a class='btn-swagger' href='http://localhost:5002/swagger'
         target='_blank'>Open Swagger</a>
    </div>

    <div class='service-card leave'>
      <div class='svc-info'>
        <div class='svc-name'>🏖 Leave Service</div>
        <div class='svc-desc'>Leave requests, Balance, Types, Holidays, SAGA</div>
      </div>
      <span class='svc-port'>:5003</span>
      <a class='btn-swagger' href='http://localhost:5003/swagger'
         target='_blank'>Open Swagger</a>
    </div>

    <div class='service-card timesheet'>
      <div class='svc-info'>
        <div class='svc-name'>⏱ Timesheet Service</div>
        <div class='svc-desc'>Daily logs, Weekly submit, Projects, Approval</div>
      </div>
      <span class='svc-port'>:5004</span>
      <a class='btn-swagger' href='http://localhost:5004/swagger'
         target='_blank'>Open Swagger</a>
    </div>

    <div class='service-card notif'>
      <div class='svc-info'>
        <div class='svc-name'>🔔 Notification Service</div>
        <div class='svc-desc'>In-app & Email notifications, Templates</div>
      </div>
      <span class='svc-port'>:5005</span>
      <a class='btn-swagger' href='http://localhost:5005/swagger'
         target='_blank'>Open Swagger</a>
    </div>

    <div class='service-card report'>
      <div class='svc-info'>
        <div class='svc-name'>📊 Report Service</div>
        <div class='svc-desc'>Leave/Timesheet reports, Excel export</div>
      </div>
      <span class='svc-port'>:5006</span>
      <a class='btn-swagger' href='http://localhost:5006/swagger'
         target='_blank'>Open Swagger</a>
    </div>

    <div class='info-box'>
      💡 <strong>How to test APIs:</strong><br/>
      1. Open Auth Service → POST /api/auth/login → copy token<br/>
      2. Open any service → click Authorize 🔒 → paste
         <code>Bearer {token}</code><br/>
      3. Test protected endpoints with the token ✅<br/>
      <br/>
      🌐 <strong>Angular Frontend:</strong>
      <a href='http://localhost:4200' target='_blank'>
        http://localhost:4200
      </a>
    </div>
  </div>
  <div class='footer'>
    LTMA — Leave & Timesheet Management Application |
    API Gateway v1.0 | ASP.NET Core 8
  </div>
</div>
</body>
</html>";

        return Content(html, "text/html");
    }

    // Health check endpoint
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "Gateway Running",
            timestamp = DateTime.UtcNow,
            port = 5000,
            services = new[]
            {
                new { name = "AuthService",         port = 5001 },
                new { name = "EmployeeService",     port = 5002 },
                new { name = "LeaveService",        port = 5003 },
                new { name = "TimesheetService",    port = 5004 },
                new { name = "NotificationService", port = 5005 },
                new { name = "ReportService",       port = 5006 }
            }
        });
    }
}