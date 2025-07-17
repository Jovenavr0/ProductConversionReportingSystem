using Application.Services;
using Application.Settings;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace ApplicationTest;

public class ReportRequestServiceTests
{
    private readonly Mock<IReportRepository> _mockReport = new ();
    private readonly Mock<IReportDecorationService> _mockDecorator = new();
    private readonly Mock<IOptions<CacheSettings>> _cacheSettingsMock = new();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly ReportRequestService _service;

    public ReportRequestServiceTests()
    {   
        _cacheSettingsMock.Setup(x => x.Value).Returns(new CacheSettings
        {
            ExpirationHoursPayload = 24,
            ExpirationHoursReports = 0.5
        });
        _service = new ReportRequestService(_cache, _mockReport.Object, _mockDecorator.Object, _cacheSettingsMock.Object);
    }

    [Fact]
    public async Task Execute_ShouldReturnCachedReport_WhenExistsInCache()
    {
        var reportId = Guid.NewGuid().ToString();
        var report = new Report
        {
            Id = Guid.Parse(reportId),
            Status = ReportStatus.Completed
        };
        var cacheKey = $"report_{reportId}";
        _cache.Set(cacheKey, report);
        
        var result = await _service.Execute(reportId);
        
        Assert.Equal(reportId, result.ReportId);
        Assert.Equal(ReportStatus.Completed, report.Status);
        _mockReport.Verify(r => r.FindReportAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Execute_ShouldReturnThrow_WhenReportNotExists()
    {
        var reportId = Guid.NewGuid().ToString();
        _mockReport.Setup(r => r.FindReportAsync(Guid.Parse(reportId))).ReturnsAsync((Report)null);
        
        await Assert.ThrowsAsync<RpcException>(async () => await _service.Execute(reportId));
    }
}