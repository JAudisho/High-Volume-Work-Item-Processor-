using AtlasQueue.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtlasQueue.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<WorkItemEvent> WorkItemEvents => Set<WorkItemEvent>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(b);
    }
}