namespace Domain.Interfaces;

public interface IPaymentRepository
{
    Task<int> GetPaymentsCountInGapAsync(long messageProductId, DateTime start, DateTime end, CancellationToken stoppingToken);
}