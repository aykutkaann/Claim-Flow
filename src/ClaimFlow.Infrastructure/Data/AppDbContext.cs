using ClaimFlow.Application.Interfaces;
using ClaimFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ClaimFlow.Infrastructure.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Premium> Premia { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<Coverage> Coverages { get; set; }
        public DbSet<Beneficiary> Beneficiaries  { get; set; }

        //claim entities

        public DbSet<Claim> Claims { get; set; }
        public DbSet<ClaimStatusHistory> Histories { get; set; }
        public DbSet<ClaimDocument> Documents { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }

    }
}
