using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using NotificationService.Application.DTOs.NotificationDTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationController(INotificationService service)
        => _service = service;

    private int GetUserId() =>
        int.TryParse(User.FindFirst("userId")?.Value, out var id) ? id : 0;

    // ── FR-NOTIF-001 + FR-NOTIF-005 ──────────────────────────────────────────

    // GET /api/notifications — all roles get their own notifications
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var list = await _service.GetMyNotificationsAsync(GetUserId());
        return Ok(list);
    }

    // GET /api/notifications/unread-count
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var count = await _service.GetUnreadCountAsync(GetUserId());
        return Ok(new { unreadCount = count });
    }

    // PUT /api/notifications/{id}/read — mark single as read
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _service.MarkAsReadAsync(id, GetUserId());
        return Ok(new { message = "Marked as read." });
    }

    // PUT /api/notifications/read-all — FR-NOTIF-005: bulk mark all as read
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _service.MarkAllAsReadAsync(GetUserId());
        return Ok(new { message = "All notifications marked as read." });
    }

    // ── FR-NOTIF-003: Template management (HR Admin only) ────────────────────

    // GET /api/notifications/templates
    [Authorize(Policy = "HROrAbove")]
    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates()
    {
        var templates = await _service.GetTemplatesAsync();
        return Ok(templates);
    }

    // PUT /api/notifications/templates/{id}
    [Authorize(Policy = "HROrAbove")]
    [HttpPut("templates/{id}")]
    public async Task<IActionResult> UpdateTemplate(
        int id, [FromBody] UpdateTemplateDto dto)
    {
        var result = await _service.UpdateTemplateAsync(id, dto);
        return Ok(result);
    }
}