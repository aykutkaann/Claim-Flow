using ClaimFlow.Application.DTOs;
using ClaimFlow.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Tenants.Queries.GetTenants
{
    public class GetTenantsHandler : IRequestHandler<GetTenantsQuery , List<TenantDto>>
    {
        private readonly IAppDbContext _context;

        public GetTenantsHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TenantDto>> Handle(GetTenantsQuery query,CancellationToken  cancellationToken)
        {

            

            return await _context.Tenants
                 .AsNoTracking()
                 .Select(t => new TenantDto(t.Id, t.Name, t.Code, t.IsActive, t.CreatedAt)) 
                 .ToListAsync(cancellationToken);



        }
    }
}
