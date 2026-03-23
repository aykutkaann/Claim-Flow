using ClaimFlow.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Policies.Commands.CreatePolicy
{
    public record CreatePolicyCommand(string PolicyNumber, ProductType ProductType, PolicyStatus PolicyStatus,
        Guid CustomerId, DateTime StartDate, DateTime EndDate) : IRequest<Guid>;
    
    
}
