using ClaimFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Infrastructure.Data.Configurations
{
    public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
    {

        public void  Configure(EntityTypeBuilder<Claim> builder)
        {
            builder.ToTable("claims");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.ClaimNumber)
                .IsRequired().HasMaxLength(100);

            builder.Property(c => c.Description)
                .IsRequired().HasMaxLength(2000);

            builder.Property(c => c.ClaimedAmount)
                .HasPrecision(18, 2).IsRequired();

            builder.Property(c => c.ApprovedAmount)
               .HasPrecision(18, 2);

            builder.Property(c => c.Status)
                .IsRequired().HasConversion<string>();

            builder.Property(c => c.SubmittedAt).HasColumnType("timestamptz");

            builder.Property(c => c.ResolvedAt).HasColumnType("timestamptz");

            builder.HasIndex(c => c.ClaimNumber)
                .IsUnique();



            builder.HasOne(c => c.Policy)
                .WithMany(p => p.Claims)
                .HasForeignKey(c => c.PolicyId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.HasOne(c => c.Tenant)
                .WithMany(p => p.Claims)
                .HasForeignKey(t => t.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Histories)
                .WithOne(c => c.Claim)
                .HasForeignKey(c => c.ClaimId);

            builder.HasMany(c => c.Documents)
               .WithOne(c => c.Claim)
               .HasForeignKey(c => c.ClaimId);



        }
    }
}
