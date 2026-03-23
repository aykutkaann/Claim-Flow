using ClaimFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Infrastructure.Data.Configurations
{
    public class ClaimDocumentConfiguration : IEntityTypeConfiguration<ClaimDocument>
    {

        public void Configure(EntityTypeBuilder<ClaimDocument> builder)
        {
            builder.ToTable("claim_docs");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.FileName)
                .IsRequired().HasMaxLength(100);

            builder.Property(d => d.FilePath)
                .IsRequired().HasMaxLength(100);

            builder.Property(d => d.UploadedAt).HasColumnType("timestamptz");


            builder.HasOne(d => d.Claim)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.ClaimId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
