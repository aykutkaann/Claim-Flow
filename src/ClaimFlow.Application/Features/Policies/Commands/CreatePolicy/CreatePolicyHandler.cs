using ClaimFlow.Application.Interfaces;
using ClaimFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Policies.Commands.CreatePolicy
{
    public class CreatePolicyHandler : IRequestHandler<CreatePolicyCommand, Guid>
    {
        private readonly IAppDbContext _context;

        public CreatePolicyHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreatePolicyCommand request, CancellationToken cancellationToken)
        {
            var customer = await _context.Customers
              .AsNoTracking()
              .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);

            if (customer == null)
                throw new Exception($"Customer not found ID: {request.CustomerId}");



            var policy = new Policy
            {
                Id = Guid.NewGuid(),
                PolicyNumber = request.PolicyNumber,
                ProductType = request.ProductType,
                PolicyStatus = request.PolicyStatus,

                StartDate = request.StartDate,
                EndDate = request.EndDate,

                CustomerId = request.CustomerId,
                TenantId = customer.TenantId


            };

            _context.Policies.Add(policy);

            await _context.SaveChangesAsync(cancellationToken);

            return policy.Id;
                
        }
    }
}
