using Microsoft.AspNetCore.Mvc;
using UserService.Data;
using UserService.Models;
using UserService.Services;
using Microsoft.EntityFrameworkCore;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly RedisCacheService _cache;

        public UserController(UserDbContext context, RedisCacheService cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            string cacheKey = $"user:{id}";
            var cachedUser = await _cache.GetAsync<User>(cacheKey);

            if (cachedUser != null)
                return Ok(new { source = "cache", data = cachedUser });

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            await _cache.SetAsync(cacheKey, user, TimeSpan.FromMinutes(1));
            return Ok(new { source = "db", data = user });
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Name = updatedUser.Name;
            user.PhoneNumber = updatedUser.PhoneNumber;
            user.Address = updatedUser.Address;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            string cacheKey = "all_users";
            var cachedUsers = await _cache.GetAsync<List<User>>(cacheKey);

            if (cachedUsers != null)
                return Ok(new { source = "cache", data = cachedUsers });

            var users = await _context.Users.ToListAsync();
            await _cache.SetAsync(cacheKey, users, TimeSpan.FromMinutes(1));

            return Ok(new { source = "db", data = users });
        }
    }
}