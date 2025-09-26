using AtlasQueue.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasQueue.Infrastructure.Persistence.Config;

public class WorkItemEventConfig : IEntityTypeConfiguration<WorkItemEvent>
{
    public void Configure(EntityTypeBuilder<WorkItemEvent> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.Type).HasMaxLength(32).IsRequired();
        e.HasIndex(x => new { x.WorkItemId, x.CreatedUtc });
    }
}