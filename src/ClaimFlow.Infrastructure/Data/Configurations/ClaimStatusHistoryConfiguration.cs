using ClaimFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Infrastructure.Data.Configurations
{
    public class ClaimStatusHistoryConfiguration : IEntityTypeConfiguration<ClaimStatusHistory>
    {

        public void Configure(EntityTypeBuilder<ClaimStatusHistory> builder)
        {
            builder.ToTable("claim_status_history");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.FromStatus)
                .IsRequired().HasMaxLength(100).HasConversion<string>();

            builder.Property(c => c.ToStatus)
              .IsRequired().HasMaxLength(100).HasConversion<string>();

            builder.Property(c => c.ChangedAt).HasColumnType("timestamptz");

            builder.Property(c => c.ChangedBy)
                .IsRequired().HasMaxLength(100);

            builder.Property(c => c.Notes)
               .HasMaxLength(500);


            builder.HasOne(c => c.Claim)
                .WithMany(cl => cl.Histories)
                .HasForeignKey(c => c.ClaimId)
                .OnDelete(DeleteBehavior.Restrict);
                
        }
    }
}
