using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OutboxRepository(AppDbContext dbContext) : IOutboxRepository
{
    public async Task<List<OutboxMessage>> GetOutboxMessagesAsync(int batchSize, CancellationToken stoppingToken)
    {
        return await dbContext.OutboxMessages
            .Where(m => !m.Processed)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(stoppingToken);
    }

    public async Task UpdateAsync(OutboxMessage message, CancellationToken stoppingToken)
    {
        dbContext.OutboxMessages.Update(message);
        await dbContext.SaveChangesAsync(stoppingToken);
    }

    public async Task UpdateRangeAsync(IEnumerable<OutboxMessage> messages, CancellationToken stoppingToken)
    {
        dbContext.OutboxMessages.UpdateRange(messages);
        await dbContext.SaveChangesAsync(stoppingToken);
    }
}