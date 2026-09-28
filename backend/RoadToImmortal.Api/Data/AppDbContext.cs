using Microsoft.EntityFrameworkCore;
using RoadToImmortal.Api.Models;

namespace RoadToImmortal.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Player> Players => Set<Player>();

    public DbSet<Match> Matches => Set<Match>();

    public DbSet<Hero> Heroes => Set<Hero>();

    public DbSet<MmrSnapshot> MmrSnapshots => Set<MmrSnapshot>();
}