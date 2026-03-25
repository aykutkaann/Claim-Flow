using ClaimFlow.Application.Interfaces;
using ClaimFlow.Domain.Entities;
using ClaimFlow.Domain.Enums;
using ClaimFlow.Domain.Events;
using ClaimFlow.Domain.StateMachines;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ClaimFlow.Application.Features.Claims.TransitionClaimCmd
{
    public class TransitionClaimHandler : IRequestHandler<TransitionClaimCommand, Unit>
    {
        private readonly IAppDbContext _context;

        public TransitionClaimHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(TransitionClaimCommand request, CancellationToken cancellationToken)
        {
            var claim = await _context.Claims
                .FirstOrDefaultAsync(c => c.Id == request.ClaimId, cancellationToken);

            if (claim == null)
                throw new Exception("Claim not found.");

            var fromStatus = claim.Status;

            var machine = new ClaimStateMachine(claim);
            machine.Fire(request.Trigger);

            var toStatus = claim.Status;

            if (request.Trigger == ClaimTrigger.Approve || request.Trigger == ClaimTrigger.Reject)
            {
                claim.ResolvedAt = DateTime.UtcNow;
            }

            var history = new ClaimStatusHistory
            {
                Id = Guid.NewGuid(),
                ClaimId = request.ClaimId,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                ChangedBy = request.ChangedBy,
                Notes = request.Notes
            };

            _context.Histories.Add(history);

            // Outbox: general transition event
            var transitionEvent = new ClaimTransitionedEvent(
                claim.Id,
                fromStatus.ToString(),
                toStatus.ToString(),
                request.ChangedBy,
                request.Notes);

            _context.Messages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = nameof(ClaimTransitionedEvent),
                Content = JsonSerializer.Serialize(transitionEvent),
                OccuredAt = DateTime.UtcNow
            });

            // Outbox: specific events for approve/reject
            if (request.Trigger == ClaimTrigger.Approve)
            {
                var approvedEvent = new ClaimApprovedEvent(
                    claim.Id,
                    claim.ClaimNumber,
                    claim.ApprovedAmount ?? claim.ClaimedAmount);

                _context.Messages.Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = nameof(ClaimApprovedEvent),
                    Content = JsonSerializer.Serialize(approvedEvent),
                    OccuredAt = DateTime.UtcNow
                });
            }
            else if (request.Trigger == ClaimTrigger.Reject)
            {
                var rejectedEvent = new ClaimRejectedEvent(
                    claim.Id,
                    claim.ClaimNumber,
                    request.Notes ?? "No reason provided");

                _context.Messages.Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = nameof(ClaimRejectedEvent),
                    Content = JsonSerializer.Serialize(rejectedEvent),
                    OccuredAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
