using LeaveService.Application.DTOs;
using LeaveService.Application.DTOs.LeaveBalanceDtos;
using LeaveService.Application.Interfaces;
using LeaveService.Domain.Entities;


namespace LeaveService.Application.Services;

public class LeaveBalanceService : ILeaveBalanceService
{
    private readonly ILeaveBalanceRepository _balanceRepo;
    private readonly ILeaveTypeRepository _typeRepo;
    private readonly ILogger<LeaveBalanceService> _logger;

    public LeaveBalanceService(
    ILeaveBalanceRepository balanceRepo,
    ILeaveTypeRepository typeRepo,
    ILogger<LeaveBalanceService> logger)
    {
        _balanceRepo = balanceRepo;
        _typeRepo = typeRepo;
        _logger = logger;
    }

    public async Task<IEnumerable<LeaveBalanceDto>> GetMyBalancesAsync(
        int userId, int? year = null)
        => await GetByUserIdAsync(userId, year);

    public async Task<IEnumerable<LeaveBalanceDto>> GetByUserIdAsync(
        int userId, int? year = null)
    {
        var y = year ?? DateTime.UtcNow.Year;
        var balances = await _balanceRepo.GetAllByUserAsync(userId, y);
        return balances.Select(Map);
    }

    // FR-LB-004: HR manually adjusts balance with reason
    public async Task AdjustAsync(AdjustBalanceDto dto)
    {
        var year = DateTime.UtcNow.Year;
        var balance = await _balanceRepo.GetAsync(dto.UserId, dto.LeaveTypeId, year);

        if (balance == null)
        {
            _ = await _typeRepo.GetByIdAsync(dto.LeaveTypeId)
                ?? throw new InvalidOperationException(
                    $"Leave type {dto.LeaveTypeId} not found.");

            await _balanceRepo.AddAsync(new LeaveBalance
            {
                UserId = dto.UserId,
                LeaveTypeId = dto.LeaveTypeId,
                Year = year,
                Entitled = Math.Max(0, dto.Adjustment),
                AdjustmentReason = dto.Reason,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            if (balance.Entitled + dto.Adjustment < 0)
                throw new InvalidOperationException(
                    "Adjustment results in negative balance.");

            balance.Entitled += dto.Adjustment;
            balance.AdjustmentReason = dto.Reason;
            balance.UpdatedAt = DateTime.UtcNow;
            await _balanceRepo.UpdateAsync(balance);
        }
    }
    public async Task InitializeBalancesForNewUserAsync(
    int userId, int year, string? gender = null)
    {
        var leaveTypes = await _typeRepo.GetAllActiveAsync();

        foreach (var lt in leaveTypes)
        {
            // ── Gender restriction check ──────────────────────────────────────
            // Skip Maternity Leave for non-female employees
            if (!string.IsNullOrEmpty(lt.GenderApplicability))
            {
                // If employee gender is unknown, skip gender-restricted leaves
                if (string.IsNullOrEmpty(gender)) continue;

                if (!lt.GenderApplicability.Equals(
                        gender, StringComparison.OrdinalIgnoreCase))
                    continue;
            }

            // ── Idempotency — skip if already exists ──────────────────────────
            var existing = await _balanceRepo.GetAsync(userId, lt.Id, year);
            if (existing != null) continue;

            await _balanceRepo.AddAsync(new LeaveBalance
            {
                UserId = userId,
                LeaveTypeId = lt.Id,
                Year = year,
                Entitled = lt.MaxDaysPerYear,
                Used = 0,
                Pending = 0,
                Carried = 0,
                AdjustmentReason = "Initial allocation",
                CreatedAt = DateTime.UtcNow
            });
        }

        _logger.LogInformation(
            "Leave balances initialized for UserId={UserId}, Year={Year}, Gender={Gender}",
            userId, year, gender ?? "unspecified");
    }

    // FR-LB-005: Year-end carry-forward
    public async Task ProcessCarryForwardAsync(int fromYear, int toYear)
    {
        var leaveTypes = await _typeRepo.GetAllAsync();

        foreach (var lt in leaveTypes.Where(x => x.IsActive))
        {
            var fromBalances = await _balanceRepo
                .GetAllByLeaveTypeAsync(lt.Id, fromYear);

            foreach (var from in fromBalances)
            {
                var unused = from.Entitled - from.Used;
                var carryFwd = Math.Min(Math.Max(unused, 0), lt.CarryForwardMax);
                var existing = await _balanceRepo
                    .GetAsync(from.UserId, lt.Id, toYear);

                if (existing == null)
                    await _balanceRepo.AddAsync(new LeaveBalance
                    {
                        UserId = from.UserId,
                        LeaveTypeId = lt.Id,
                        Year = toYear,
                        Entitled = lt.MaxDaysPerYear,
                        Carried = carryFwd,
                        CreatedAt = DateTime.UtcNow
                    });
                else
                {
                    existing.Carried = carryFwd;
                    existing.UpdatedAt = DateTime.UtcNow;
                    await _balanceRepo.UpdateAsync(existing);
                }
            }
        }
    }

    private static LeaveBalanceDto Map(LeaveBalance b) => new()
    {
        LeaveTypeName = b.LeaveType?.Name ?? string.Empty,
        LeaveTypeCode = b.LeaveType?.Code ?? string.Empty,
        Year = b.Year,
        Entitled = b.Entitled,
        Carried = b.Carried,
        Used = b.Used,
        Pending = b.Pending,
        Available = b.Available,
        IsPaid = b.LeaveType?.IsPaid ?? true
    };
}