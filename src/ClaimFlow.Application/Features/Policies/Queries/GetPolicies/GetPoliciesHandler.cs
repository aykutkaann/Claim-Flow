using ClaimFlow.Application.DTOs;
using ClaimFlow.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Policies.Queries.GetPolicies
{
    public class GetPoliciesHandler : IRequestHandler<GetPoliciesQuery, List<PolicyDto>>
    {
        private readonly IAppDbContext _context;

        public GetPoliciesHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PolicyDto>> Handle(GetPoliciesQuery request, CancellationToken cancellationToken)
        {
            return  await _context.Policies
                .AsNoTracking()
                .Select(p => new PolicyDto(
                        p.Id,
                        p.PolicyNumber,
                        p.ProductType.ToString(), 
                        p.PolicyStatus.ToString(),
                        p.StartDate,
                        p.EndDate,
                        p.CustomerId
                        ))
                .ToListAsync(cancellationToken);

            


        }
    }
}
