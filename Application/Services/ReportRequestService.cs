using Application.DTOs;
using Application.Interfaces;
using Application.Settings;
using Domain.Entities;
using Domain.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class ReportRequestService(IMemoryCache memoryCache, IReportRepository reportRepository, IReportDecorationService reportDecorationService, IOptions<CacheSettings> cacheSettings) : IReportRequestService
{
    private readonly double _expirationHours = cacheSettings.Value.ExpirationHoursReports;
    
    public async Task<ReportResponseDto> Execute(string reportId)
    {
        var cacheKey = GenerateCacheKey(reportId);
        var report = memoryCache.Get<Report>(cacheKey);

        if (report != null)
        {
            return new ReportResponseDto(reportId, report.Status);
        }

        report = await reportRepository.FindReportAsync(Guid.Parse(reportId));
        
        if (report == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Report not found"));
        }
        
        reportDecorationService.GenerateDecorationReport(report);
        memoryCache.Set(cacheKey, report, TimeSpan.FromHours(_expirationHours));
        
        return new ReportResponseDto(reportId, report.Status);
    }

    private static string GenerateCacheKey(string reportId)
    {
        return $"report_{reportId}";
    }
}