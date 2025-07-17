using Domain.Entities;

namespace Domain.Interfaces;

public interface IBillingRepository
{
    Task AddOperationAsync(BillingOperation operation);
    BillingReport? GetReportByUserId(string userId);
    Task UpdateReportAsync(BillingReport report);
}