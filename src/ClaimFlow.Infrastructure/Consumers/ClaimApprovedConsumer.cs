using ClaimFlow.Application.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Infrastructure.Consumers
{
    public class ClaimApprovedConsumer : IConsumer<ClaimApprovedMessage>
    {
        private readonly ILogger<ClaimApprovedConsumer> _logger;

        public ClaimApprovedConsumer(ILogger<ClaimApprovedConsumer> logger)
        {
            _logger = logger;   
        }

        public Task Consume(ConsumeContext<ClaimApprovedMessage> context)
        {
            _logger.LogInformation("RabbitMQ: Claim {ClaimNumber} received. Would notify adjuster.", context.Message.ClaimNumber);

            return Task.CompletedTask;
        }
    }
}
