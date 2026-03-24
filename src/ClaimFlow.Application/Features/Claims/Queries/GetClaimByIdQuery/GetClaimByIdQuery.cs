using ClaimFlow.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Claims.Queries.GetClaimByIdQuery
{
    public record GetClaimByIdQuery(Guid ClaimId) : IRequest<ClaimDetailDto?>;

    public record ClaimDetailDto(
                    Guid Id,
                    string ClaimNumber,
                    string Description,
                    decimal ClaimedAmount,
                    ClaimStatus Status,
                    DateTime CreatedAt,
                    DateTime? ResolvedAt,
                    List<ClaimHistoryDto> History);

    public record ClaimHistoryDto(
                    ClaimStatus FromStatus,
                    ClaimStatus ToStatus,
                    ClaimTrigger Trigger,
                    string ChangedBy,
                    string? Notes,
                    DateTime CreatedAt);


}
