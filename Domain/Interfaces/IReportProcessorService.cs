using Domain.Entities;

namespace Domain.Interfaces;

public interface IReportProcessorService
{
    Task ProcessMessageAsync(OutboxMessage message, CancellationToken stoppingToken);
}