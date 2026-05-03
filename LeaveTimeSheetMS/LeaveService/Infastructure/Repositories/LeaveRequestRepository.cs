using LeaveService.Application.Interfaces;
using LeaveService.Domain.Entities;
using LeaveService.Domain.Enums;
using LeaveService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeaveService.Infrastructure.Repositories;

public class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly LeaveDbContext _ctx;
    public LeaveRequestRepository(LeaveDbContext ctx) => _ctx = ctx;

    public async Task<LeaveRequest?> GetByIdAsync(int id)
        => await _ctx.LeaveRequests
            .Include(x => x.LeaveType)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<IEnumerable<LeaveRequest>> GetByUserIdAsync(int userId)
        => await _ctx.LeaveRequests
            .Include(x => x.LeaveType)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<LeaveRequest>> GetPendingForManagerAsync()
    {
        // ✅ FIX 2 (Part D): Previously this fetched ALL Status="Pending" leaves,
        // which INCLUDED leaves that the manager already approved but are now
        // waiting for HR (NeedsHrApproval=true). After the manager approves a
        // >5-day leave, NeedsHrApproval is set to true — those rows must be
        // EXCLUDED from the manager queue or they appear as if still pending.
        var leaves = await _ctx.LeaveRequests
            .Include(l => l.LeaveType)
            .Where(l => l.Status == "Pending" && !l.NeedsHrApproval)  // ✅ KEY FIX
            .ToListAsync();

        // Get CorrelationIds from saga table
        var leaveIds = leaves.Select(l => l.Id).ToList();
        var sagas = await _ctx.Set<LeaveApprovalSagaState>()
            .Where(s => leaveIds.Contains(s.LeaveId))
            .ToListAsync();

        foreach (var leave in leaves)
        {
            var saga = sagas.FirstOrDefault(s => s.LeaveId == leave.Id);
            leave.CorrelationId = saga?.CorrelationId;
        }

        return leaves;
    }

    public async Task<IEnumerable<LeaveRequest>> GetPendingHrApprovalAsync()
    {
        // ✅ FIX 2 (Part E): Previously this method did NOT fetch CorrelationIds
        // from the Saga table. The frontend's hrApprove() and hrReject() both
        // pass leave.correlationId to the API — but without this join the
        // correlationId was always null, causing "Missing correlationId." error
        // in the frontend and a 400 on the backend (correlationId=Guid.Empty).
        var leaves = await _ctx.LeaveRequests
            .Include(x => x.LeaveType)
            .Where(x => x.NeedsHrApproval && x.Status == "Pending")  // ✅ added Status check
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        // ✅ FIX 2 (Part E): Join with SAGA table to get CorrelationIds
        var leaveIds = leaves.Select(l => l.Id).ToList();
        var sagas = await _ctx.Set<LeaveApprovalSagaState>()
            .Where(s => leaveIds.Contains(s.LeaveId))
            .ToListAsync();

        foreach (var leave in leaves)
        {
            var saga = sagas.FirstOrDefault(s => s.LeaveId == leave.Id);
            leave.CorrelationId = saga?.CorrelationId;
        }

        return leaves;
    }

    public async Task<bool> HasOverlapAsync(int userId,
        DateTime start, DateTime end, int? excludeId = null)
        => await _ctx.LeaveRequests.AnyAsync(x =>
            x.UserId == userId &&
            (excludeId == null || x.Id != excludeId) &&
            x.Status != LeaveStatus.Rejected &&
            x.Status != LeaveStatus.Cancelled &&
            x.Status != LeaveStatus.Withdrawn &&
            x.StartDate <= end.Date &&
            x.EndDate >= start.Date);

    public async Task AddAsync(LeaveRequest r)
    {
        await _ctx.LeaveRequests.AddAsync(r);
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateAsync(LeaveRequest r)
    {
        _ctx.LeaveRequests.Update(r);
        await _ctx.SaveChangesAsync();
    }
}