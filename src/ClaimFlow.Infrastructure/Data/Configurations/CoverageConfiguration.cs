using ClaimFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Infrastructure.Data.Configurations
{
    public class CoverageConfiguration : IEntityTypeConfiguration<Coverage>
    {
        public void Configure(EntityTypeBuilder<Coverage> builder)
        {
            builder.ToTable("coverages");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.CoverageType).IsRequired();

            builder.Property(c => c.MaxAmount).HasPrecision(18, 2).IsRequired();

            builder.Property(c => c.DeductibleAmount).HasPrecision(18, 2).IsRequired();

            //many-to-one

            builder.HasOne(c => c.Policy)
                .WithMany(pol => pol.Coverages)
                .HasForeignKey(c => c.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
