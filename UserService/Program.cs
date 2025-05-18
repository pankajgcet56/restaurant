using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UserService.Data;
using UserService.Services;

namespace UserService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers();
            
            builder.Services.AddDbContext<UserDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("UserDb")));

            builder.Services.AddScoped<IUserService, UserService.Services.UserService>();

            // Use InMemory DB (can change to SQL Server later)
            // builder.Services.AddDbContext<UserDbContext>(options =>
            //     options.UseInMemoryDatabase("UserDb"));

            // Redis configuration
            builder.Services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect("localhost:6379,abortConnect=false"));
            builder.Services.AddScoped<RedisCacheService>();

            var app = builder.Build();
            
            // Apply any pending migrations automatically on app startup
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                db.Database.Migrate();  // <-- This runs migrations to create/update schema
            }

            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}