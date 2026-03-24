using System;
using System.Collections.Generic;
using System.Text;


namespace ClaimFlow.Domain.Events
{

    public record ClaimApprovedEvent(Guid ClaimId, decimal ApprovedAmount) : MediatR.INotification;
}
