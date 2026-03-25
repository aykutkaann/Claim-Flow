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
        private readonly IEmbeddingService _embeddingService;
        private readonly IFraudDetectionService _fraudDetectionService;

        public SubmitClaimHandler(
            IAppDbContext context,
            IEmbeddingService embeddingService,
            IFraudDetectionService fraudDetectionService)
        {
            _context = context;
            _embeddingService = embeddingService;
            _fraudDetectionService = fraudDetectionService;
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

            // Generate embedding from claim description
            var embeddingArray = await _embeddingService.GetEmbeddingAsync(claim.Description);
            claim.Embedding = new Pgvector.Vector(embeddingArray);

            _context.Claims.Add(claim);

            // Save claim + embedding first (fraud check needs the claim in DB)
            await _context.SaveChangesAsync(cancellationToken);

            // Run fraud detection
            var fraudResult = await _fraudDetectionService.CheckFraudAsync(claim.Id);
            claim.FraudRiskScore = fraudResult.FraudRiskScore;

            if (fraudResult.FraudRiskScore > 60)
            {
                claim.IsFraud = true;
            }

            // Save event to outbox
            var claimEvent = new ClaimSubmittedEvent(
                claim.Id,
                claim.ClaimNumber,
                claim.PolicyId,
                claim.TenantId,
                claim.Description,
                claim.ClaimedAmount);

            _context.Messages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = nameof(ClaimSubmittedEvent),
                Content = JsonSerializer.Serialize(claimEvent),
                OccuredAt = DateTime.UtcNow
            });

            // Save fraud score update + outbox message
            await _context.SaveChangesAsync(cancellationToken);

            return claim.Id;
        }

        private string GenerateClaimNumber()
        {
            return $"CLM-{DateTime.Now.Year}-{new Random().Next(1000, 9999)}";
        }
    }
}
