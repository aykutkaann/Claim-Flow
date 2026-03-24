using ClaimFlow.Domain.Entities;
using ClaimFlow.Domain.Events;
using ClaimFlow.Infrastructure.Data;
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
            var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

            var messages = await context.Messages
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.OccuredAt)
                .Take(20)
                .ToListAsync(stoppingToken);

            foreach (var message in messages)
            {
                try
                {
                    var notification = DeserializeEvent(message.Type, message.Content);

                    if (notification != null)
                    {
                        await publisher.Publish(notification, stoppingToken);
                        _logger.LogInformation("Published outbox message {Type} for {Id}.", message.Type, message.Id);
                    }

                    message.ProcessedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process outbox message {Id}.", message.Id);
                    message.Error = ex.Message;
                }
            }

            if (messages.Count > 0)
                await context.SaveChangesAsync(stoppingToken);
        }

        private INotification? DeserializeEvent(string type, string content)
        {
            return type switch
            {
                nameof(ClaimSubmittedEvent) => JsonSerializer.Deserialize<ClaimSubmittedEvent>(content),
                nameof(ClaimTransitionedEvent) => JsonSerializer.Deserialize<ClaimTransitionedEvent>(content),
                nameof(ClaimApprovedEvent) => JsonSerializer.Deserialize<ClaimApprovedEvent>(content),
                nameof(ClaimRejectedEvent) => JsonSerializer.Deserialize<ClaimRejectedEvent>(content),
                _ => null
            };
        }
    }
}
