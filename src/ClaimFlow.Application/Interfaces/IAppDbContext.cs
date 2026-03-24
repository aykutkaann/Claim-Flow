using ClaimFlow.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Application.Interfaces
{
    public interface IAppDbContext
    {
        
        DbSet<Tenant> Tenants { get; }
        DbSet<Customer> Customers { get; }
        DbSet<Policy> Policies { get; }
        DbSet<Premium> Premia { get; }
        DbSet<Coverage> Coverages { get; }
        DbSet<Beneficiary> Beneficiaries { get; }


        //Claim entities
        DbSet<Claim> Claims { get; }
        DbSet<ClaimStatusHistory> Histories { get; }
        DbSet<ClaimDocument> Documents { get; }


        DbSet<OutboxMessage> Messages { get; }


        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    }
}
