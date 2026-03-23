using ClaimFlow.Application.DTOs;
using ClaimFlow.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Customers.Queries.GetCustomers
{
    public class GetCustomersHandler : IRequestHandler<GetCustomersQuery, List<CustomerDto>>
    {
        private readonly IAppDbContext _context;

        public GetCustomersHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CustomerDto>> Handle(GetCustomersQuery query, CancellationToken cancellationToken)
        {
            return await _context.Customers
                .AsNoTracking()
                .Select(c => new CustomerDto(
                c.Id,
                c.FullName, 
                c.Email,
                c.TenantId,                    
                c.CreatedAt                   
            ))
            .ToListAsync(cancellationToken);

            


        }
    }
}
