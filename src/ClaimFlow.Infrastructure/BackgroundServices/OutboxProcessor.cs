using ClaimFlow.Application.Messages;
using ClaimFlow.Domain.Entities;
using ClaimFlow.Domain.Events;
using ClaimFlow.Infrastructure.Data;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ClaimFlow.Infrastructure.BackgroundServices
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxProcessor> _logger;

        public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxMessages(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox messages.");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private async Task ProcessOutboxMessages(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            var messages = await context.Messages
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.OccuredAt)
                .Take(20)
                .ToListAsync(stoppingToken);

            foreach (var message in messages)
            {
                try
                {
                    await PublishMessageBasedOnType(message, publishEndpoint, stoppingToken);

                    message.ProcessedAt = DateTime.UtcNow;
                    _logger.LogInformation("Successfully published outbox message {Type} to message broker.", message.Type);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish outbox message {Id} to broker.", message.Id);
                    message.Error = ex.Message;
                }
            }

            if (messages.Count > 0)
                await context.SaveChangesAsync(stoppingToken);
        }

        private async Task PublishMessageBasedOnType(OutboxMessage message, IPublishEndpoint publishEndpoint, CancellationToken ct)
        {
            switch (message.Type)
            {
                case nameof(ClaimSubmittedEvent):
                    var subEvt = JsonSerializer.Deserialize<ClaimSubmittedEvent>(message.Content);
                    if (subEvt != null)
                    {
                        await publishEndpoint.Publish(new ClaimSubmittedMessage(
                            subEvt.ClaimId, subEvt.ClaimNumber, subEvt.PolicyId, subEvt.TenantId, subEvt.Description, subEvt.ClaimedAmount), ct);
                    }
                    break;

                case nameof(ClaimApprovedEvent):
                    var appEvt = JsonSerializer.Deserialize<ClaimApprovedEvent>(message.Content);
                    if (appEvt != null)
                    {
                        await publishEndpoint.Publish(new ClaimApprovedMessage(
                            appEvt.ClaimId, appEvt.ClaimNumber, appEvt.ApprovedAmount), ct);
                    }
                    break;

                case nameof(ClaimRejectedEvent):
                    var rejEvt = JsonSerializer.Deserialize<ClaimRejectedEvent>(message.Content);
                    if (rejEvt != null)
                    {
                        await publishEndpoint.Publish(new ClaimRejectedMessage(
                            rejEvt.ClaimId, rejEvt.ClaimNumber, rejEvt.Reason), ct);
                    }
                    break;

                case nameof(ClaimTransitionedEvent):
                    var transEvt = JsonSerializer.Deserialize<ClaimTransitionedEvent>(message.Content);
                    if (transEvt != null)
                    {
                        await publishEndpoint.Publish(new ClaimTransitionedMessage(
                            transEvt.ClaimId, transEvt.FromStatus, transEvt.ToStatus, transEvt.ChangedBy), ct);
                    }
                    break;
                default:
                    _logger.LogWarning("Unknown outbox message type: {Type}", message.Type);
                    break;



            }
        }
    }
}