using ClaimFlow.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Application.Features.Claims.EventHandlers
{
    public class ClaimSubmittedEventHandler : INotificationHandler<ClaimSubmittedEvent>
    {

        private readonly ILogger<ClaimSubmittedEventHandler> _logger;

        public ClaimSubmittedEventHandler(ILogger<ClaimSubmittedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(ClaimSubmittedEvent notification, CancellationToken cancellationToken)
        {
            

            _logger.LogInformation("Claim {ClaimNumber} submitted. Adjuster notification would be sent here.", notification.ClaimNumber);

            return Task.CompletedTask;
        }

    }
}
