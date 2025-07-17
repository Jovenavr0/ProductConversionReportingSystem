using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PaymentRepository(AppDbContext dbContext) : IPaymentRepository
{
    public async Task<int> GetPaymentsCountInGapAsync(long messageProductId, DateTime start, DateTime end, CancellationToken stoppingToken)
    {
        return await dbContext.Payments.CountAsync(v => 
            v.ProductId == messageProductId &&
            v.Timestamp >= start &&
            v.Timestamp <= end, stoppingToken
        );
    }
}