using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Messages
{
    public class ClaimSubmittedMessage
    {
        public Guid ClaimId { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public Guid PolicyId { get; set; }
        public Guid TenantId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal ClaimedAmount { get; set; }
    }

    public class ClaimTransitionedMessage
    {
        public Guid ClaimId { get; set; }
        public string FromStatus { get; set; } = string.Empty;
        public string ToStatus { get; set; } = string.Empty;
        public string ChangedBy { get; set; } = string.Empty;
    }

    public class ClaimApprovedMessage
    {
        public Guid ClaimId { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public decimal ApprovedAmount { get; set; }
    }

    public class ClaimRejectedMessage
    {
        public Guid ClaimId { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
