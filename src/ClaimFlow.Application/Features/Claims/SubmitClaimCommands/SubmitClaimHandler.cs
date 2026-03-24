using ClaimFlow.Application.Interfaces;
using ClaimFlow.Domain.Entities;
using ClaimFlow.Domain.Enums;
using ClaimFlow.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ClaimFlow.Application.Features.Claims.Commands
{
    public class SubmitClaimHandler : IRequestHandler<SubmitClaimCommand, Guid>
    {
        private readonly IAppDbContext _context;

        public SubmitClaimHandler(IAppDbContext context)
        {
            _context = context;
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

            // Save event to outbox — same transaction as the claim
            var claimEvent = new ClaimSubmittedEvent(
                claim.Id,
                claim.ClaimNumber,
                claim.PolicyId,
                claim.TenantId);

            _context.Messages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = nameof(ClaimSubmittedEvent),
                Content = JsonSerializer.Serialize(claimEvent),
                OccuredAt = DateTime.UtcNow
            });

            // One SaveChangesAsync = one transaction = both claim AND event saved together
            await _context.SaveChangesAsync(cancellationToken);

            return claim.Id;
        }

        private string GenerateClaimNumber()
        {
            return $"CLM-{DateTime.Now.Year}-{new Random().Next(1000, 9999)}";
        }
    }
}
