using AtlasQueue.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasQueue.Infrastructure.Persistence.Config;

public class WorkItemConfig : IEntityTypeConfiguration<WorkItem>
{
    public void Configure(EntityTypeBuilder<WorkItem> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.ExternalRef).IsRequired().HasMaxLength(128);
        e.Property(x => x.PayloadJson).IsRequired();
        e.HasIndex(x => new { x.Status, x.Priority, x.CreatedUtc });
        e.HasMany(x => x.Events).WithOne().HasForeignKey(ev => ev.WorkItemId);
    }
}