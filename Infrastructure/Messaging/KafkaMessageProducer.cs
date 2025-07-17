using Application.Settings;
using Confluent.Kafka;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Messaging.Serializers;
using Microsoft.Extensions.Options;

namespace Infrastructure.Messaging;

public class KafkaMessageProducer : IMessageProducer, IDisposable
{
    private readonly IProducer<Null, OutboxMessage> _producer;

    public KafkaMessageProducer(IOptions<BusSettings> kafkaSettings)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = kafkaSettings.Value.BootstrapServers
        };
        _producer = new ProducerBuilder<Null, OutboxMessage>(producerConfig).SetValueSerializer(new JsonSerializer<OutboxMessage>()).Build();
    }
    
    public async Task ProduceAsync(string topic, OutboxMessage message, CancellationToken stoppingToken)
    {
        await _producer.ProduceAsync(topic, new Message<Null, OutboxMessage> { Value = message }, stoppingToken);
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}