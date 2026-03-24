using ClaimFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Infrastructure.Data.Configurations
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("outbox_message");

            builder.HasIndex(o => o.ProcessedAt);

            builder.Property(o => o.Type).IsRequired();

            builder.Property(o => o.Content).IsRequired();

            builder.Property(o => o.OccuredAt).HasColumnType("timestamptz");

            builder.Property(o => o.ProcessedAt).HasColumnType("timestamptz");

            builder.Property(o => o.Error).IsRequired(false);

        }
    }
}
