using Microsoft.EntityFrameworkCore;
using CharacterApi.Models;
namespace CharacterApi.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Character> Characters => Set<Character>();
    }
}
