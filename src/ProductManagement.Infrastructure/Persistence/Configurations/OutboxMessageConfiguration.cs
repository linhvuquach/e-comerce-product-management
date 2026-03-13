using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id");
        builder.Property(o => o.Type).HasColumnName("type").HasMaxLength(500).IsRequired();
        builder.Property(o => o.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        builder.Property(o => o.OccurredAt).HasColumnName("occurred_at").IsRequired();
        builder.Property(o => o.ProcessedAt).HasColumnName("processed_at");

        builder.HasIndex(o => o.ProcessedAt)
            .HasFilter("processed_at IS NULL")
            .HasDatabaseName("idx_outbox_unprocessed");
    }
}
