using ClaimFlow.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Policies.Queries.GetPolicies
{
    public record GetPoliciesQuery : IRequest<List<PolicyDto>>;
    
    
}
