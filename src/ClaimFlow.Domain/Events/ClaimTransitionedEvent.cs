using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace ClaimFlow.Domain.Events
{
    public record ClaimTransitionedEvent(Guid ClaimId, string FromStatus, string ToStatus, string ChangedBy, string? Notes) : INotification;

}
