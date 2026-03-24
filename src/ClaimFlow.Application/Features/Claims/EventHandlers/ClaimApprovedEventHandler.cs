using ClaimFlow.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Claims.EventHandlers
{
    public class ClaimApprovedEventHandler : INotificationHandler<ClaimApprovedEvent>
    {

        private readonly ILogger<ClaimApprovedEventHandler> _logger;

        public ClaimApprovedEventHandler(ILogger<ClaimApprovedEventHandler> logger)
        {
            _logger = logger;
        }

        public  Task Handle(ClaimApprovedEvent notification, CancellationToken cancellationToken)
        {

            _logger.LogInformation("Claim {ClaimId} approved for {Amount}. Payment process would start here.", notification.ClaimId, notification.ApprovedAmount);

            return Task.CompletedTask;

        }
    }
}
