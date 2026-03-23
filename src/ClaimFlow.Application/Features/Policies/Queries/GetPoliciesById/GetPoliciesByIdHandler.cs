using ClaimFlow.Application.DTOs;
using ClaimFlow.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Policies.Queries.GetPoliciesById
{
    public class GetPoliciesByIdHandler : IRequestHandler<GetPoliciesByIdQuery, PolicyDto>
    {

        private readonly IAppDbContext _context;

        public GetPoliciesByIdHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<PolicyDto> Handle(GetPoliciesByIdQuery query, CancellationToken cancellationToken)
        {
            var p = await _context.Policies
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == query.Id, cancellationToken);

            return new PolicyDto(p.Id,
                                    p.PolicyNumber,
                                    p.ProductType.ToString(),
                                    p.PolicyStatus.ToString(),
                                    p.StartDate,
                                    p.EndDate,
                                    p.CustomerId);
        }
    }
}
