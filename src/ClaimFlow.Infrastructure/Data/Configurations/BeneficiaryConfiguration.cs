using ClaimFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Infrastructure.Data.Configurations
{
    public class BeneficiaryConfiguration: IEntityTypeConfiguration<Beneficiary>
    {

        public void Configure(EntityTypeBuilder<Beneficiary> builder)
        {
            builder.ToTable("beneficiaries");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.FullName).IsRequired().HasMaxLength(100);

            builder.Property(b => b.Relationships).IsRequired().HasMaxLength(100);

            builder.Property(b => b.Percentage).IsRequired().HasPrecision(18, 2);

            //many-to-one

            builder.HasOne(b => b.Policy)
                 .WithMany(pol => pol.Beneficiaries)
                 .HasForeignKey(b => b.PolicyId)
                 .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
