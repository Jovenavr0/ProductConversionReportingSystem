using Application.Settings;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class BillingService(IBillingRepository billingRepository, IOptions<BillingSettings> billingSettings) : IBillingService
{
    private readonly TimeSpan _freePeriod = TimeSpan.FromHours(billingSettings.Value.FreePeriodHours);
    private readonly decimal _reportCost = billingSettings.Value.ReportCost;

    public async Task InitializeReportRequest(string userId, string reportId)
    {
        if (!NeedToPay(userId)) 
            return;
        
        var operation = new BillingOperation
        {
            UserId = userId,
            Amount = _reportCost,
            OperationType = "GenerationReport",
            Descrition = $"Generating report for report {reportId}"
        };
            
        await billingRepository.AddOperationAsync(operation);
        await UpdateBillingReport(userId);
    }

    private async Task UpdateBillingReport(string userId)
    {
        var billingReport = billingRepository.GetReportByUserId(userId);

        if (billingReport == null)
        {
            billingReport = new BillingReport
            {
                UserId = userId,
                LastPaymentTime = DateTime.UtcNow 
            };
        }
        else
        {
            billingReport.LastPaymentTime = DateTime.UtcNow;
        }

        await billingRepository.UpdateReportAsync(billingReport);
    }

    private bool NeedToPay(string userId)
    {
        var lastPayment = billingRepository.GetReportByUserId(userId);

        if (lastPayment == null)
        {
            return true;
        }
        
        return DateTime.UtcNow - lastPayment.LastPaymentTime > _freePeriod;
    }
}