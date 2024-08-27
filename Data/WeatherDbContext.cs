using Microsoft.EntityFrameworkCore;
using WeatherAppAPI.Models;

namespace WeatherAppAPI.Data
{
    public class WeatherDbContext : DbContext
    {
        public WeatherDbContext(DbContextOptions<WeatherDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
