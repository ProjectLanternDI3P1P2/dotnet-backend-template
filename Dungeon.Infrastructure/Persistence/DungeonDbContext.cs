using Dungeon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dungeon.Infrastructure.Persistence;

public class DungeonDbContext(DbContextOptions<DungeonDbContext> options) : DbContext(options)
{
    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<DungeonRun> DungeonRuns { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Applique toutes les configurations d'entités automatiquement
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DungeonDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
