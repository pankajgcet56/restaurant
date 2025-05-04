using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models;

namespace UserService.Services;

public class UserService : IUserService
{
    private readonly UserDbContext _context;

    public UserService(UserDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> UpdateUserAsync(int id, User user)
    {
        var existing = await _context.Users.FindAsync(id);
        if (existing == null) return false;

        existing.Name = user.Name;
        existing.PhoneNumber = user.PhoneNumber;
        existing.Address = user.Address;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}