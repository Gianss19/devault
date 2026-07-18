using devalut.Models;
using Microsoft.EntityFrameworkCore;

namespace devalut.Entities.Persistance;

public class DevaultDbContext : DbContext
{
    public DevaultDbContext(DbContextOptions options) : base(options)
    {
    }

    protected DevaultDbContext()
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DevaultDbContext).Assembly);
    }

  public DbSet<User> Users => Set<User>();
  public DbSet<Secret> Secrets => Set<Secret>();
  public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

}
