using ClaimFlow.Application.Interfaces;
using ClaimFlow.Domain.Entities;
using ClaimFlow.Domain.Enums;
using ClaimFlow.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Claims.Commands
{
    public class SubmitClaimHandler : IRequestHandler<SubmitClaimCommand, Guid>
    {
        private readonly IAppDbContext _context;
        private readonly IPublisher _publisher;

        public SubmitClaimHandler(IAppDbContext context, IPublisher publisher)
        {
            _context = context;
            _publisher = publisher;
        }

        public async Task<Guid> Handle(SubmitClaimCommand request, CancellationToken cancellationToken)
        {
            var policy = await _context.Policies
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.PolicyId, cancellationToken);

            if (policy == null)
                throw new Exception("Policy not found.");

            if (policy.PolicyStatus != PolicyStatus.Active)
                throw new Exception("Cant create a claim record on a pasif policy.");

            var claim = new Claim
            {
                Id = Guid.NewGuid(),
                ClaimNumber = GenerateClaimNumber(),
                PolicyId = request.PolicyId,
                TenantId = policy.TenantId,
                Description = request.Description,
                ClaimedAmount = request.ClaimedAmount,
                Status = ClaimStatus.Submitted,
                SubmittedAt = DateTime.UtcNow


            };

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync(cancellationToken);

            await _publisher.Publish(new ClaimSubmittedEvent(
                claim.Id,
                claim.ClaimNumber,
                claim.PolicyId,
                claim.TenantId), cancellationToken);

            return claim.Id;
        }

        private string GenerateClaimNumber()
        {
            return $"CLM-{DateTime.Now.Year}-{new Random().Next(1000, 9999)}";
        }

    }
}
