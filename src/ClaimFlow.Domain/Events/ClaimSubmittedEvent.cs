using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace ClaimFlow.Domain.Events
{
    public record ClaimSubmittedEvent(Guid ClaimId, string ClaimNumber, Guid PolicyId, Guid TenantId) : INotification;

}
