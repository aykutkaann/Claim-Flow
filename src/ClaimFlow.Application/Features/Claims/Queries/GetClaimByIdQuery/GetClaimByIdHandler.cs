using ClaimFlow.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.Application.Features.Claims.Queries.GetClaimByIdQuery
{
    public class GetClaimByIdHandler : IRequestHandler<GetClaimByIdQuery, ClaimDetailDto?>
    {
        private readonly IAppDbContext _context;

        public GetClaimByIdHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ClaimDetailDto?> Handle(GetClaimByIdQuery request, CancellationToken cancellationToken)
        {
            var claim = await _context.Claims
                .Include(c => c.Histories)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.ClaimId, cancellationToken);

            if (claim == null)
                return null;

            var historyDtos = claim.Histories
                .OrderBy(h => h.ChangedAt)
                .Select(h => new ClaimHistoryDto(
                    h.FromStatus,
                    h.ToStatus,
                    default,
                    h.ChangedBy,
                    h.Notes,
                    h.ChangedAt))
                .ToList();

            return new ClaimDetailDto(
                claim.Id,
                claim.ClaimNumber,
                claim.Description,
                claim.ClaimedAmount,
                claim.Status,
                claim.SubmittedAt,
                claim.ResolvedAt,
                historyDtos);
        }
    }
}
