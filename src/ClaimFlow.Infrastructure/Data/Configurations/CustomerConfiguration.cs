using ClaimFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Infrastructure.Data.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {

            builder.ToTable("customers");

            

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(c => c.FullName)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(c => c.Email)
                   .IsRequired()
                   .HasMaxLength(150);
            builder.Property(c => c.ProfileData).HasColumnType("jsonb");


            builder.HasIndex(c => c.Email)
                .IsUnique();

            builder.HasOne(c => c.Tenant)
                .WithMany(t => t.Customers)
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
