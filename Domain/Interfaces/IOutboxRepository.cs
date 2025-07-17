using Domain.Entities;

namespace Domain.Interfaces;

public interface IOutboxRepository
{
    Task<List<OutboxMessage>> GetOutboxMessagesAsync(int batchSize, CancellationToken stoppingToken);
    
    Task UpdateAsync(OutboxMessage message, CancellationToken stoppingToken);
    
    Task UpdateRangeAsync(IEnumerable<OutboxMessage> messages, CancellationToken stoppingToken);
}