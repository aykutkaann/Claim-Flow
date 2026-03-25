using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Messages
{
    public record ClaimSubmittedMessage(
        Guid ClaimId,
        string ClaimNumber,
        Guid PolicyId,
        Guid TenantId,
        string Description,
        decimal ClaimedAmount
        );

    public record ClaimTransitionedMessage(
        Guid ClaimId,
        string FromStatus,
        string ToStatus,
        string ChangedBy
        );

    public record ClaimApprovedMessage(
        Guid ClaimId,
        string ClaimNumber,
        decimal ApprovedAmount
        );

    public record ClaimRejectedMessage(
        Guid ClaimId,
        string ClaimNumber,
        string Reason
        );
}
