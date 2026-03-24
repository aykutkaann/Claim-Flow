using ClaimFlow.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Claims.TransitionClaimCmd
{
    public record TransitionClaimCommand(Guid ClaimId, ClaimTrigger Trigger, string ChangedBy, string? Notes) : IRequest<Unit>;
    
    
}
