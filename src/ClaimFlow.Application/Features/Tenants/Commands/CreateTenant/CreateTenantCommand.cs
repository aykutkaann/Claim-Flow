using ClaimFlow.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Tenants.Commands.CreateTenant
{
    public record CreateTenantCommand(string Name, string Code) : IRequest<Guid>;
   
}
