using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Domain.Enums
{
    public enum  ClaimStatus
    {
        Submitted,
        UnderReview,
        DocumentsRequested,
        UnderInvestigation,
        Approved,
        Rejected,
        PaymentScheduled,
        Paid,
        Closed,
        Appeal
    }
}
