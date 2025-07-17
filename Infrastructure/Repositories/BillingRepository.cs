using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Repositories;

public class BillingRepository(AppDbContext dbContext) : IBillingRepository
{
    public async Task AddOperationAsync(BillingOperation operation)
    {
        dbContext.BillingOperations.Add(operation);
        await dbContext.SaveChangesAsync();
    }

    public BillingReport? GetReportByUserId(string userId)
    {
        return dbContext.BillingReports.FirstOrDefault(r => r.UserId == userId);
    }

    public async Task UpdateReportAsync(BillingReport report)
    {
        dbContext.BillingReports.Update(report);
        await dbContext.SaveChangesAsync();
    }

}