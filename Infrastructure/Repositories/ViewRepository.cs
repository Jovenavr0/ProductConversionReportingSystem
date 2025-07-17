using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ViewRepository(AppDbContext dbContext) : IViewRepository
{
    public async Task<int> GetViewsCountInGapAsync(long messageProductId, DateTime start, DateTime end, CancellationToken stoppingToken)
    {
        return await dbContext.Views.CountAsync(v => v.ProductId == messageProductId &&
            v.Timestamp >= start &&
            v.Timestamp <= end, stoppingToken
        );
    }
}