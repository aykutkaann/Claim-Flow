using ClaimFlow.Application.Interfaces;
using ClaimFlow.Domain.Entities;
using ClaimFlow.Domain.Enums;
using ClaimFlow.Domain.Events;
using ClaimFlow.Domain.StateMachines;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Application.Features.Claims.TransitionClaimCmd
{
    public class TransitionClaimHandler : IRequestHandler<TransitionClaimCommand, Unit>
    {
        private readonly IAppDbContext _context;
        private readonly IPublisher _publisher;

        public TransitionClaimHandler(IAppDbContext context, IPublisher publisher)
        {
            _context = context;
            _publisher = publisher;
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

            await _context.SaveChangesAsync(cancellationToken);

            // Publish the general transition event
            await _publisher.Publish(new ClaimTransitionedEvent(
                claim.Id,
                fromStatus.ToString(),
                toStatus.ToString(),
                request.ChangedBy,
                request.Notes), cancellationToken);

            // Publish specific events for approve/reject
            if (request.Trigger == ClaimTrigger.Approve)
            {
                await _publisher.Publish(new ClaimApprovedEvent(
                    claim.Id,
                    claim.ApprovedAmount ?? claim.ClaimedAmount), cancellationToken);
            }
            else if (request.Trigger == ClaimTrigger.Reject)
            {
                await _publisher.Publish(new ClaimRejectedEvent(
                    claim.Id,
                    request.Notes ?? "No reason provided"), cancellationToken);
            }

            return Unit.Value;
        }
    }
}
