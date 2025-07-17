using Domain.Entities;

namespace Domain.Interfaces;

public interface IMessageProducer
{
    Task ProduceAsync(string topic, OutboxMessage message, CancellationToken stoppingToken);
}