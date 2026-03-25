using System;
using System.Collections.Generic;
using System.Text;


namespace ClaimFlow.Domain.Events
{

    public record ClaimApprovedEvent(
        Guid ClaimId,
        string ClaimNumber,
        decimal ApprovedAmount) : MediatR.INotification;
}
