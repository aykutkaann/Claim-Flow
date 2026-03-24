using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Domain.Enums
{
    public enum  ClaimTrigger
    {

        StartReview,
        RequestDocuments,
        StartInvestigation,
        Approve,
        Reject,
        SchedulePayment,
        ConfirmPayment,
        Close,
        FileAppeal
    }
}
