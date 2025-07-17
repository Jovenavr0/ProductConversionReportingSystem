using Application.Settings;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Workers;

public class OutboxWorker (ILogger<OutboxWorker> logger, IServiceProvider serviceProvider, IOptions<BusSettings> busSettings) : BackgroundService
{
    private readonly int _batchSize = busSettings.Value.BatchSize;
    private readonly string _topicName = busSettings.Value.Topics.ReportRequests;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                var messageProducer = scope.ServiceProvider.GetRequiredService<IMessageProducer>();
                
                var messages = await outboxRepository.GetOutboxMessagesAsync(_batchSize, stoppingToken);

                if (messages.Count == 0)
                {
                    await Task.Delay(5000, stoppingToken);
                    continue;
                }
                
                var processedMessages = new List<OutboxMessage>();
                
                foreach (var message in messages)
                {
                    try
                    {
                        await messageProducer.ProduceAsync(_topicName, message, stoppingToken);
                        message.Processed = true;
                        message.ProcessedAt = DateTime.UtcNow;
                        processedMessages.Add(message);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing outbox messages: {messageId}", message.Id);
                    }
                }

                if (processedMessages.Count > 0)
                {
                    await outboxRepository.UpdateRangeAsync(processedMessages, stoppingToken);
                    logger.LogInformation("Processed {processedMessagesCount} outbox messages",
                        processedMessages.Count);
                }

                await Task.Delay(2000, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("ReportWorker is stopping.");
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "Not all services are transferred to scope in OutboxWorker");
                await Task.Delay(10000, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error pin outbox worker");
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}