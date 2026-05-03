using LeaveService.Application.DTOs;
using LeaveService.Application.DTOs.LeaveBalanceDtos;
using LeaveService.Application.DTOs.LeaveRequestDtos;
using LeaveService.Application.DTOs.LeaveTypeDtos;
using LeaveService.Application.Interfaces;
using LeaveService.Domain.Enums;

using MassTransit;
using Shared.Events.Saga;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LeaveService.Controllers;

[ApiController]
[Route("api/leaves")]
[Authorize]
public class LeaveController : ControllerBase
{
    private readonly ILeaveRequestService _leaveService;
    private readonly ILeaveTypeService _typeService;
    private readonly ILeaveBalanceService _balanceService;
    private readonly HttpClient _httpClient;
    private readonly IPublishEndpoint _publishEndpoint;


    public LeaveController(
        ILeaveRequestService leaveService,
        ILeaveTypeService typeService,
        ILeaveBalanceService balanceService,
        IPublishEndpoint publishEndpoint,
         IHttpClientFactory httpClientFactory)
    {
        _leaveService = leaveService;
        _typeService = typeService;
        _balanceService = balanceService;
        _publishEndpoint = publishEndpoint;
        _httpClient = httpClientFactory.CreateClient();
    }

    private int GetUserId() =>
        int.TryParse(User.FindFirst("userId")?.Value, out var id) ? id : 0;

    private string GetRole() =>
        User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

    // ── LEAVE TYPES ─────────────────────────────────────────────

    [HttpGet("types")]
    public async Task<IActionResult> GetLeaveTypes()
        => Ok(await _typeService.GetAllAsync());

    [Authorize(Policy = "HROrAbove")]
    [HttpPost("types")]
    public async Task<IActionResult> CreateLeaveType(CreateLeaveTypeDto dto)
        => StatusCode(201, await _typeService.CreateAsync(dto));

    [Authorize(Policy = "HROrAbove")]
    [HttpPut("types/{id}")]
    public async Task<IActionResult> UpdateLeaveType(int id, UpdateLeaveTypeDto dto)
        => Ok(await _typeService.UpdateAsync(id, dto));

    [Authorize(Policy = "HROrAbove")]
    [HttpPut("types/{id}/deactivate")]
    public async Task<IActionResult> DeactivateLeaveType(int id)
    {
        await _typeService.DeactivateAsync(id);
        return Ok(new { message = "Leave type deactivated." });
    }

    [Authorize(Policy = "HROrAbove")]
    [HttpPut("types/{id}/activate")]
    public async Task<IActionResult> ActivateLeaveType(int id)
    {
        await _typeService.ActivateAsync(id);
        return Ok(new { message = "Leave type activated." });
    }

    // ── LEAVE BALANCE ───────────────────────────────────────────

    [HttpGet("balance")]
    [Authorize]
    public async Task<IActionResult> GetMyBalance([FromQuery] int? year = null)
    {
        var userId = GetUserId();
        var y = year ?? DateTime.UtcNow.Year;

        // Safety net: initialize if no balances exist yet
        var existing = await _balanceService.GetByUserIdAsync(userId, y);
        if (!existing.Any())
        {
            var gender = await GetEmployeeGenderAsync(userId);
            await _balanceService.InitializeBalancesForNewUserAsync(userId, y, gender);
        }

        var balances = await _balanceService.GetByUserIdAsync(userId, y);
        return Ok(balances);
    }

    private async Task<string?> GetEmployeeGenderAsync(int userId)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"http://localhost:5002/api/employees/internal/gender/{userId}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadFromJsonAsync<GenderResponse>();
            return json?.Gender;
        }
        catch { return null; }
    }

    private record GenderResponse(string? Gender);

    [Authorize(Policy = "HROrAbove")]
    [HttpGet("balance/{userId}")]
    public async Task<IActionResult> GetBalanceByUser(int userId, int? year = null)
        => Ok(await _balanceService.GetByUserIdAsync(userId, year));

    [Authorize(Policy = "HROrAbove")]
    [HttpPost("balance/adjust")]
    public async Task<IActionResult> AdjustBalance(AdjustBalanceDto dto)
    {
        await _balanceService.AdjustAsync(dto);
        return Ok(new { message = "Balance adjusted." });
    }

    // ── LEAVE REQUESTS (SAGA ENABLED) ───────────────────────────

    // ✅ START SAGA
    [HttpPost]
    public async Task<IActionResult> Apply(CreateLeaveRequestDto dto)
    {
        var result = await _leaveService.ApplyAsync(GetUserId(), dto);
        return StatusCode(201, result);
    }


    // GET LEAVES
    [HttpGet]
    public async Task<IActionResult> GetLeaves()
    {
        return Ok(await _leaveService.GetMyLeavesAsync(GetUserId()));
    }

    // GET BY ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var leave = await _leaveService.GetByIdAsync(id);
        if (leave == null) return NotFound();

        if (GetRole() == UserRoles.Employee && leave.UserId != GetUserId())
            return Forbid();

        return Ok(leave);
    }

    // GET MANAGER PENDING
    [Authorize(Policy = "ManagerOrAbove")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
        => Ok(await _leaveService.GetPendingForManagerAsync());

    // ✅ MANAGER APPROVES → SAGA
    // ✅ FIX 2 (Part A): The Saga handles state transitions correctly but it
    // was never updating LeaveRequest.Status in the DB when Days > 5.
    // The ApproveLeaveBalanceConsumer DOES update status to Approved for the
    // no-HR path. For the HR path the Saga transitions to WaitingHrApproval
    // but no consumer was updating the DB record to reflect that.
    // Solution: After publishing to SAGA, we ALSO call the service to update
    // the DB record status and set NeedsHrApproval flag directly, so the
    // leave is removed from manager-pending and appears in hr-pending.
    [Authorize(Policy = "ManagerOrAbove")]
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(int id, [FromQuery] Guid correlationId)
    {
        // Step 1: Publish to SAGA (handles notifications + balance)
        await _publishEndpoint.Publish(new ManagerApprovedLeave
        {
            CorrelationId = correlationId,
            LeaveId = id,
            ManagerId = GetUserId()
        });

        // ✅ FIX 2 (Part A): Also update the DB record directly so the leave
        // moves out of manager-pending queue. For leaves > 5 days this sets
        // NeedsHrApproval=true and keeps Status=Pending (WaitingHrApproval).
        // For leaves <= 5 days the ApproveLeaveBalanceConsumer will set
        // Status=Approved asynchronously — this call ensures the manager flag is set.
        await _leaveService.ApproveAsync(id, GetUserId());

        return Ok(new { message = "Approval sent to workflow." });
    }

    // ✅ MANAGER REJECTS → SAGA
    [Authorize(Policy = "ManagerOrAbove")]
    [HttpPut("{id}/reject")]
    public async Task<IActionResult> Reject(int id,
        [FromQuery] Guid correlationId,
        RejectLeaveDto dto)
    {
        await _publishEndpoint.Publish(new ManagerRejectedLeave
        {
            CorrelationId = correlationId,
            LeaveId = id,
            ManagerId = GetUserId(),
            Comment = dto.Comment
        });

        // Also update the DB record
        await _leaveService.RejectAsync(id, GetUserId(), dto.Comment);

        return Ok(new { message = "Rejection sent to workflow." });
    }

    // CANCEL (still direct)
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        await _leaveService.CancelAsync(id, GetUserId());
        return Ok(new { message = "Cancelled." });
    }

    // ── HR SECOND-LEVEL ENDPOINTS ────────────────────────────────
    // ✅ FIX 2 (Part B): These three endpoints were COMPLETELY MISSING from
    // the controller. The frontend was calling /api/leaves/hr-pending,
    // /api/leaves/{id}/hr-approve, and /api/leaves/{id}/hr-reject — but none
    // of these routes existed, so every HR action returned 404.

    /// <summary>
    /// Returns leaves that have been manager-approved but need HR second-level
    /// approval (Days > 5 and NeedsHrApproval = true).
    /// </summary>
    [Authorize(Policy = "HROrAbove")]
    [HttpGet("hr-pending")]
    public async Task<IActionResult> GetHrPending()
        => Ok(await _leaveService.GetPendingHrAsync());

    /// <summary>
    /// HR approves a leave that is in WaitingHrApproval state.
    /// Publishes HrApprovedLeave to the SAGA which then publishes
    /// ApproveLeaveBalance to update the balance.
    /// Also updates the DB record directly.
    /// </summary>
    [Authorize(Policy = "HROrAbove")]
    [HttpPut("{id}/hr-approve")]
    public async Task<IActionResult> HrApprove(int id, [FromQuery] Guid correlationId)
    {
        // Step 1: Publish to SAGA (handles balance update + notification)
        await _publishEndpoint.Publish(new HrApprovedLeave
        {
            CorrelationId = correlationId,
            LeaveId = id,
            HrId = GetUserId()
        });

        // Step 2: Update DB record directly so it's removed from hr-pending
        await _leaveService.HrApproveAsync(id, GetUserId());

        return Ok(new { message = "HR approval sent to workflow." });
    }

    /// <summary>
    /// HR rejects a leave that is in WaitingHrApproval state.
    /// Publishes HrRejectedLeave to the SAGA which runs the compensating
    /// transaction (restores balance). Also updates the DB record directly.
    /// </summary>
    [Authorize(Policy = "HROrAbove")]
    [HttpPut("{id}/hr-reject")]
    public async Task<IActionResult> HrReject(int id,
        [FromQuery] Guid correlationId,
        RejectLeaveDto dto)
    {
        // Step 1: Publish to SAGA (runs compensating transaction)
        await _publishEndpoint.Publish(new HrRejectedLeave
        {
            CorrelationId = correlationId,
            LeaveId = id,
            HrId = GetUserId(),
            Comment = dto.Comment
        });

        // Step 2: Update DB record directly
        await _leaveService.HrRejectAsync(id, GetUserId(), dto.Comment);

        return Ok(new { message = "HR rejection sent to workflow." });
    }
}