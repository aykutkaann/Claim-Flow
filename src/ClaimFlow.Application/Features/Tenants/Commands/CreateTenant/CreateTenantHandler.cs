using ClaimFlow.Application.Features.Customers.Commands.CreateCustomer;
using ClaimFlow.Application.Interfaces;
using ClaimFlow.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Tenants.Commands.CreateTenant
{
    public class CreateTenantHandler : IRequestHandler<CreateTenantCommand, Guid>
    {
        private readonly IAppDbContext _context;

        public CreateTenantHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
        {
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Code = request.Code
            };

            _context.Tenants.Add(tenant);

            await _context.SaveChangesAsync(cancellationToken);

            return tenant.Id;
        }
    }

}
