using ClaimFlow.Application.Interfaces;
using ClaimFlow.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Customers.Commands.CreateCustomer
{
    public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Guid>
    {

        private readonly IAppDbContext _context;

        public CreateCustomerHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
               FullName = request.FullName,
               Email = request.Email,
               TenantId = request.TenantId
                
            };

            _context.Customers.Add(customer);

            await _context.SaveChangesAsync(cancellationToken);

            return customer.Id;

        }
    }
}
