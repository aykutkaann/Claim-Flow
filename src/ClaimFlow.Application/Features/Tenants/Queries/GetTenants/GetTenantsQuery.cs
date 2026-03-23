using ClaimFlow.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Tenants.Queries.GetTenants
{
    public record GetTenantsQuery : IRequest<List<TenantDto>>;
    
    
}
