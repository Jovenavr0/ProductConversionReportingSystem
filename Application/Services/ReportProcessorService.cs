using System.Text.Json;
using Application.Settings;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Records;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class ReportProcessorService(ILogger<ReportProcessorService> logger, IMemoryCache memoryCache, IViewRepository viewRepository, IReportRepository reportRepository, IPaymentRepository paymentRepository, IOptions<CacheSettings> cacheSettings) : IReportProcessorService
{
    private readonly double _expirationHours = cacheSettings.Value.ExpirationHoursPayload;
    
    public async Task ProcessMessageAsync(OutboxMessage outboxMessage, CancellationToken stoppingToken)
    {
        var message = DeserializeMessage(outboxMessage.Payload);
        
        if (message == null)
        {
            return;
        }

        var cacheKey = GenerateCacheKey(message);
        var report = await GetOrLoadReportAsync(message.ReportId, cacheKey, stoppingToken);

        if (report.Status == ReportStatus.Completed)
        {
            HandleCompleteReport(report, cacheKey, "already completed");
            return;
        }
        
        var (ratio, paymentsCount) = await CalculateConversion(message.ProductId, message.StartGap, message.EndGap, viewRepository, paymentRepository, stoppingToken);
        
        await UpdateReportAsync(report, ratio, paymentsCount, reportRepository, stoppingToken);
        HandleCompleteReport(report, cacheKey, "completed");
    }

    private MessageBus? DeserializeMessage(string messageJson)
    {
        try
        {
            return JsonSerializer.Deserialize<MessageBus>(messageJson);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Error in ProcessMessageAsync");
            return null;
        }
    }

    private async Task<Report> GetOrLoadReportAsync(Guid reportId, string cacheKey, CancellationToken stoppingToken)
    {
        if (memoryCache.TryGetValue(cacheKey, out Report? existingReport))
        {
            logger.LogInformation("Using cached data for report {ReportId}", existingReport.Id);
            return existingReport;
        }
        
        var report = await reportRepository.FirstOrDefaultAsync(reportId, stoppingToken);

        if (report != null)
        {
            return report;
        }
        
        logger.LogError("Report {messageReportId} not found in database", reportId);
        throw new InvalidOperationException($"Report {reportId} not found in database");
    }

    private void HandleCompleteReport(Report report, string cacheKey, string status)
    {
        logger.LogInformation("Report {messageReportId} has been {status}", report.Id,  status);
        memoryCache.Set(cacheKey, report, TimeSpan.FromHours(_expirationHours));
    }

    private static async Task UpdateReportAsync(Report report, double ratio, int paymentsCount, IReportRepository reportRepository, CancellationToken cancellationToken)
    {
        report.Status = ReportStatus.Completed;
        report.Ratio = ratio;
        report.PaymentsCount = paymentsCount;
        await reportRepository.UpdateAsync(report, cancellationToken);
    }
    
    private static async Task<(double ratio, int paymentsCount)> CalculateConversion(long messageProductId, DateTime start, DateTime end, IViewRepository viewRepository, IPaymentRepository paymentRepository, CancellationToken stoppingToken)
    {
        var viewsTask = viewRepository.GetViewsCountInGapAsync(messageProductId, start, end, stoppingToken);
            
        var paymentsTask = paymentRepository.GetPaymentsCountInGapAsync(messageProductId, start, end, stoppingToken);
        
        await Task.WhenAll(viewsTask, paymentsTask);
        
        var views = viewsTask.Result;
        var payments = paymentsTask.Result;
        
        var ratio = views > 0 ? (double)payments / views : 0;
        return (ratio, payments);
    }  

    private static string GenerateCacheKey(MessageBus messageBus)
    {
        return $"{messageBus.ProductId}_{Timestamp.FromDateTime(messageBus.StartGap).Seconds}_{Timestamp.FromDateTime(messageBus.EndGap).Seconds}";
    }
}