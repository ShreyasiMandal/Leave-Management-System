namespace NotificationService.Application.DTOs.NotificationDTOs
{
    // ── REQUEST DTOs ──────────────────────────────────────────────────────────────

    // FR-NOTIF-004: User sets their own notification preferences
    public class NotificationPreferenceDto
    {
        public bool EmailEnabled { get; set; } = true;
        public bool InAppEnabled { get; set; } = true;
    }
}
