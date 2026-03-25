using ClaimFlow.Application.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Infrastructure.Consumers
{
    public class ClaimSubmittedConsumer : IConsumer<ClaimSubmittedMessage>
    {
        private readonly ILogger<ClaimSubmittedConsumer> _logger;

        public ClaimSubmittedConsumer(ILogger<ClaimSubmittedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<ClaimSubmittedMessage> context)
        {
            _logger.LogInformation("RabbitMQ: Claim {ClaimNumber} received. Would notify adjuster.", context.Message.ClaimNumber);

            return Task.CompletedTask;
        }
    }
}
