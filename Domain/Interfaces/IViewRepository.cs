namespace Domain.Interfaces;

public interface IViewRepository
{
    Task<int> GetViewsCountInGapAsync(long messageProductId, DateTime start, DateTime end, CancellationToken stoppingToken);
}