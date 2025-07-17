using Application.Settings;
using Confluent.Kafka;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Messaging.Serializers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Messaging;

public class KafkaMessageConsumer : IMessageConsumer, IDisposable
{
    private readonly IConsumer<Ignore, OutboxMessage> _consumer;
    private readonly ILogger<KafkaMessageConsumer> _logger;
    
    public KafkaMessageConsumer(ILogger<KafkaMessageConsumer> logger, IOptions<BusSettings> kafkaSettings)
    {
        var config = new ConsumerConfig
        {
            GroupId = kafkaSettings.Value.GroupId,
            BootstrapServers = kafkaSettings.Value.BootstrapServers,
            AutoOffsetReset = AutoOffsetReset.Latest,
            EnableAutoCommit = kafkaSettings.Value.EnableAutoCommit
        };
        _consumer = new ConsumerBuilder<Ignore, OutboxMessage>(config).SetValueDeserializer(new JsonDeserializer<OutboxMessage>()).Build();
        _logger = logger;
    }
    
    public async Task ConsumeAsync(string topic, Func<OutboxMessage, Task> messageHandler, CancellationToken stoppingToken)
    {
        _consumer.Subscribe(topic);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);
                await messageHandler(consumeResult.Message.Value);
                _consumer.Commit(consumeResult);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Consumer exception {Reason}", ex.Error.Reason);
            }
        }
    }

    public void Dispose()
    {
        _consumer.Dispose();
    }
    
}