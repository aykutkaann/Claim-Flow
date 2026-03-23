using ClaimFlow.Application.DTOs;
using ClaimFlow.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Tenants.Queries.GetTenantById
{
    public class GetTenantByIdHandler: IRequestHandler<GetTenantByIdQuery , TenantDto>
    {

        private readonly IAppDbContext _context;

        public GetTenantByIdHandler(IAppDbContext context)
        {
            _context = context;

        }

        public async Task<TenantDto> Handle(GetTenantByIdQuery query, CancellationToken cancellationToken)
        {

            var t = await _context.Tenants
             .AsNoTracking()
             .FirstOrDefaultAsync(t => t.Id == query.Id, cancellationToken);
             

          

            return new TenantDto(t.Id, t.Name, t.Code, t.IsActive, t.CreatedAt);
        }
    }
}
