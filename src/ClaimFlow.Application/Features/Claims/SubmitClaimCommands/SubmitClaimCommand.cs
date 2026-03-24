using ClaimFlow.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Claims.Commands
{
    public record SubmitClaimCommand(Guid PolicyId, string Description, decimal ClaimedAmount) : IRequest<Guid>;
    
    
}
