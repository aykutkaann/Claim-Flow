using ClaimFlow.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Tenants.Queries.GetTenantById
{
    public record GetTenantByIdQuery(Guid Id) : IRequest<TenantDto>;
    
    
}
