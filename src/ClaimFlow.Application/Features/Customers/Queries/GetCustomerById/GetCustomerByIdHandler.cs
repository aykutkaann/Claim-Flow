using ClaimFlow.Application.DTOs;
using ClaimFlow.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Customers.Queries.GetCustomerById
{
    public class GetCustomerByIdHandler: IRequestHandler<GetCustomerByIdQuery, CustomerDto>
    {
        private readonly IAppDbContext _context;

        public GetCustomerByIdHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerDto> Handle( GetCustomerByIdQuery query ,CancellationToken cancellationToken)
        {

            var c = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == query.Id, cancellationToken);


            return new CustomerDto(c.Id, c.FullName, c.Email, c.TenantId, c.CreatedAt);


        }


    }
}
