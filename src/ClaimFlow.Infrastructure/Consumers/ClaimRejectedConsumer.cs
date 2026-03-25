using ClaimFlow.Application.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClaimFlow.Infrastructure.Consumers
{
    public class ClaimRejectedConsumer : IConsumer<ClaimRejectedMessage>
    {

        private readonly ILogger<ClaimRejectedConsumer> _logger;

        public ClaimRejectedConsumer(ILogger<ClaimRejectedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<ClaimRejectedMessage> context)
        {
            _logger.LogInformation("RabbitMQ: Claim {ClaimNumber} received. Would notify adjuster.", context.Message.ClaimNumber);

            return Task.CompletedTask;

        }
    }
}
