using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;

    public UserRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLower());

    public async Task<User?> GetByIdAsync(int id)
        => await _context.Users.FindAsync(id);

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

    public async Task<User?> GetByResetTokenAsync(string token)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.PasswordResetToken == token
                && u.PasswordResetExpiry > DateTime.UtcNow);

    public async Task<bool> EmailExistsAsync(string email)
        => await _context.Users
            .AnyAsync(u => u.Email == email.ToLower());

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    => await _context.Users
        .OrderBy(u => u.FullName)
        .ToListAsync();
}