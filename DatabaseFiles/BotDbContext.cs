using Microsoft.EntityFrameworkCore;
using RGuard.Database.Models;

namespace RGuard.Database
{
    public class BotDbContext : DbContext
    {
        public BotDbContext(DbContextOptions<BotDbContext> options) : base(options) { }
        public DbSet<Guild> Guilds {get; set;}
        public DbSet<UserModel> UserModels { get; set; }
    }
}
