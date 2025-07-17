using System.Text.Json;
using Application.Services;
using Application.Settings;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Records;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace ApplicationTest;

public class ReportProcessorServiceTests
{
    private readonly Mock<ILogger<ReportProcessorService>> _logger = new();
    private readonly Mock<IMemoryCache> _memoryCache = new();
    private readonly Mock<IViewRepository> _viewRepository = new();
    private readonly Mock<IReportRepository> _reportRepository = new();
    private readonly Mock<IPaymentRepository> _paymentRepository = new();
    private readonly Mock<IOptions<CacheSettings>> _cacheSettingsMock = new();
    private readonly ReportProcessorService _reportProcessorService;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public ReportProcessorServiceTests()
    {
        _cacheSettingsMock.Setup(x => x.Value).Returns(new CacheSettings
        {
            ExpirationHoursPayload = 24,
            ExpirationHoursReports = 0.5
        });
        _reportProcessorService = new ReportProcessorService(
            _logger.Object,
            _memoryCache.Object,
            _viewRepository.Object,
            _reportRepository.Object,
            _paymentRepository.Object,
            _cacheSettingsMock.Object
        );
    }
    
    private static (string json, MessageBus message) CreateTestMessage()
    {
        var message = new MessageBus(
            Guid.NewGuid(),
            1,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1)
        );
        return (JsonSerializer.Serialize(message), message);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldUpdatedAndCachedReport_WhenReportNew()
    {
        var (json, message) = CreateTestMessage();
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            EventType = "ReportRequested",
            Payload = json,
            Processed = false
        };
        var report = new Report { Id = message.ReportId, Status = ReportStatus.Pending, DecorationId = "1" };
        var cacheKey = $"{message.ProductId}_{Timestamp.FromDateTime(message.StartGap).Seconds}_{Timestamp.FromDateTime(message.EndGap).Seconds}";
        
        _memoryCache.Setup(c => c.TryGetValue(cacheKey, out It.Ref<object>.IsAny!)).Returns(false);
        _reportRepository.Setup(r => r.FirstOrDefaultAsync(message.ReportId, _cancellationToken)).ReturnsAsync(report);
        _viewRepository.Setup(r => r.GetViewsCountInGapAsync(message.ProductId, message.StartGap, message.EndGap, _cancellationToken)).ReturnsAsync(100);
        _paymentRepository.Setup(r => r.GetPaymentsCountInGapAsync(message.ProductId, message.StartGap, message.EndGap, _cancellationToken)).ReturnsAsync(10);

        var cacheEntryMock = new Mock<ICacheEntry>();
        _memoryCache.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);
        
        await _reportProcessorService.ProcessMessageAsync(outboxMessage, _cancellationToken);

        _reportRepository.Verify(r => r.UpdateAsync(It.Is<Report>(rep =>
                rep.Status == ReportStatus.Completed &&
                rep.Ratio == 0.1 &&
                rep.PaymentsCount == 10), _cancellationToken),
            Times.Once
        );
        
        _memoryCache.Verify(c => c.CreateEntry(cacheKey), Times.Once);
    }
    
    [Fact]
    public async Task ProcessMessageAsync_ShouldGetFromCache_WhenCachedReport()
    {
        var (json, message) = CreateTestMessage();
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            EventType = "ReportRequested",
            Payload = json,
            Processed = false
        };
        var cachedReport = new Report { Id = message.ReportId, Status = ReportStatus.Completed, DecorationId = "1" };
        var cacheKey = $"{message.ProductId}_{Timestamp.FromDateTime(message.StartGap).Seconds}_{Timestamp.FromDateTime(message.EndGap).Seconds}";
        
        var cacheEntryMock = new Mock<ICacheEntry>();
        _memoryCache.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);
        
        object cachedValue = cachedReport;
        _memoryCache.Setup(c => c.TryGetValue(cacheKey, out cachedValue!)).Returns(true);
        
        await _reportProcessorService.ProcessMessageAsync(outboxMessage, _cancellationToken);

        _reportRepository.Verify(r => r.FirstOrDefaultAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldReturnNull_WhenInvalidJson()
    {
        const string invalidJson = "invalid";
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            EventType = "ReportRequested",
            Payload = invalidJson,
            Processed = false
        };
        
        await _reportProcessorService.ProcessMessageAsync(outboxMessage, _cancellationToken);
       
        _memoryCache.Verify(c => c.CreateEntry(It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldReturnThrow_WhenReportNotFound()
    {
        var (json, message) = CreateTestMessage();
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            EventType = "ReportRequested",
            Payload = json,
            Processed = false
        };
        
        _memoryCache.Setup(c => c.TryGetValue(It.IsAny<string>(), out It.Ref<object>.IsAny!)).Returns(false);
        _reportRepository.Setup(r => r.FirstOrDefaultAsync(message.ReportId, It.IsAny<CancellationToken>())).ReturnsAsync((Report)null);
        
        await Assert.ThrowsAsync<InvalidOperationException>( () => _reportProcessorService.ProcessMessageAsync(outboxMessage, _cancellationToken) );
    }

    [Fact]
    public async Task ProcessMessageAsync_ShouldReturnRatioZero_WhenZeroCountView()
    {
        var (json, message) = CreateTestMessage();
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            EventType = "ReportRequested",
            Payload = json,
            Processed = false
        };
        var report = new Report { Id = message.ReportId, Status = ReportStatus.Pending, DecorationId = "1" };
        var cacheKey = $"{message.ProductId}_{Timestamp.FromDateTime(message.StartGap).Seconds}_{Timestamp.FromDateTime(message.EndGap).Seconds}";
        
        _memoryCache.Setup(c => c.TryGetValue(cacheKey, out It.Ref<object>.IsAny)).Returns(false);
        _reportRepository.Setup(r => r.FirstOrDefaultAsync(message.ReportId, _cancellationToken)).ReturnsAsync(report);
        _viewRepository.Setup(r => r.GetViewsCountInGapAsync(message.ProductId, message.StartGap, message.EndGap, _cancellationToken)).ReturnsAsync(0);
        _paymentRepository.Setup(r => r.GetPaymentsCountInGapAsync(message.ProductId, message.StartGap, message.EndGap, _cancellationToken)).ReturnsAsync(10);

        var cacheEntryMock = new Mock<ICacheEntry>();
        _memoryCache.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);
        
        await _reportProcessorService.ProcessMessageAsync(outboxMessage, _cancellationToken);

        _reportRepository.Verify(r => r.UpdateAsync(It.Is<Report>(rep =>
                rep.Status == ReportStatus.Completed &&
                rep.Ratio == 0 &&
                rep.PaymentsCount == 10), _cancellationToken),
            Times.Once
        );
        
        _memoryCache.Verify(c => c.CreateEntry(cacheKey), Times.Once);
    }
}