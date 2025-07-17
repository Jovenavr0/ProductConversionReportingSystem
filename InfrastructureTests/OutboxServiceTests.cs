using Application.DTOs;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using Assert = Xunit.Assert;

namespace InfrastructureTests;

public class OutboxServiceTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly OutboxService _outboxService;

    public OutboxServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(databaseName: "OutboxTestDb")
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options;
        _dbContext = new AppDbContext(options);
        _outboxService = new OutboxService(_dbContext);
    }

    [Fact]
    public async Task AddReportAsync_ShouldCreateReportAndOutboxMessage()
    {
        var dto = new ReportRequestDto(
            ProductId: 123,
            StartGap: DateTime.UtcNow.AddDays(-7),
            EndGap: DateTime.UtcNow,
            UserId: "test_user",
            DecorationId: "test_decoration"
        );

        var reportId = await _outboxService.AddReportRequestAsync(dto);
        
        Assert.NotNull(reportId);
        Assert.NotEmpty(reportId);
        
        var report = await _dbContext.Reports.FindAsync(Guid.Parse(reportId));
        Assert.NotNull(report);
        Assert.Equal(ReportStatus.Pending, report.Status);
        
        var outboxMessage = await _dbContext.OutboxMessages.FirstOrDefaultAsync();
        Assert.NotNull(outboxMessage);
        Assert.Contains(reportId, outboxMessage.Payload);
    }
    
    public void Dispose()
    {
        _dbContext.Dispose();
    }
}