using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Domain.Events
{
    public record ClaimRejectedEvent(Guid ClaimId, string Reason) : MediatR.INotification;
 

   
}
