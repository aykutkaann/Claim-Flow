using ClaimFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Infrastructure.Data.Configurations
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {

        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.ToTable("tenants");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name).IsRequired().HasMaxLength(200);

            builder.Property(t => t.Code).IsRequired().HasMaxLength(50);

            builder.Property(t => t.IsActive).IsRequired().HasDefaultValue(true);
            
            builder.Property(t => t.CreatedAt).HasColumnType("timestamptz");

            builder.HasIndex(t => t.Code).IsUnique();
            
        }
    }
}
