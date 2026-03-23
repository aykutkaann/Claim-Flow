using ClaimFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Infrastructure.Data.Configurations
{
    public class PremiumConfiguration : IEntityTypeConfiguration<Premium>
    {

        public void Configure(EntityTypeBuilder<Premium> builder)
        {
            builder.ToTable("premiums");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Amount).IsRequired().HasPrecision(18, 2);

            builder.Property(p => p.DueDate).IsRequired().HasColumnType("timestamptz");

            builder.Property(p => p.PaidDate).IsRequired(false).HasColumnType("timestamptz");

            builder.Property(p => p.PremiumStatus).IsRequired();


            // many-to-one premia => many
            builder.HasOne(p => p.Policy)
                .WithMany(pol => pol.Premia)
                .HasForeignKey(p => p.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);



        }
    }
}
