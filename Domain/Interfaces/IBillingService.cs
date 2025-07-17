namespace Domain.Interfaces;

public interface IBillingService
{
    Task InitializeReportRequest(string userId, string reportId);
}