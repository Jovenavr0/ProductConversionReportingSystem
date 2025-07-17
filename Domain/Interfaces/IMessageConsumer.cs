using Domain.Entities;

namespace Domain.Interfaces;

public interface IMessageConsumer
{
    Task ConsumeAsync(string topic, Func<OutboxMessage, Task> messageHandler, CancellationToken cancellationToken);
}