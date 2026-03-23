using ClaimFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Infrastructure.Data.Configurations
{
    public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
    {
        public void Configure(EntityTypeBuilder<Policy> builder)
        {
            builder.ToTable("policies");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.PolicyNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.ProductType)
                .IsRequired();

            builder.Property(p => p.PolicyStatus)
                .IsRequired();

            builder.Property(p => p.StartDate)
                .IsRequired().HasColumnType("timestamptz");

            builder.Property(p => p.EndDate)
                .IsRequired().HasColumnType("timestamptz");



            builder.HasIndex(p => p.PolicyNumber).IsUnique();



            //many-to-one  policy => customer
            builder.HasOne(p => p.Customer)
                .WithMany(p => p.Policies)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);


            //many-to-one  policy => tenant

            builder.HasOne(p => p.Tenant)
                .WithMany(t => t.Policies)
                .HasForeignKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            
            //one-to-many Policy => covarages
            builder.HasMany(p => p.Coverages)
                .WithOne(c => c.Policy)
                .HasForeignKey(c => c.PolicyId);

            //one-to-many Policy => premia
            builder.HasMany(p => p.Premia)
                .WithOne(pr => pr.Policy)
                .HasForeignKey(pr => pr.PolicyId);
        }
    }
}
