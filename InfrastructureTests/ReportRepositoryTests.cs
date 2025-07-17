using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using Assert = Xunit.Assert;

namespace InfrastructureTests;

public class ReportRepositoryTests : IDisposable
{
    private readonly IReportRepository _repository;
    private readonly AppDbContext _dbContext;

    public ReportRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(databaseName: "TestDb")
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options;
        
        _dbContext = new AppDbContext(options); 
        _repository = new ReportRepository(_dbContext);
        
        _dbContext.OutboxMessages.AddRange();
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task FindReportAsync_ShouldReturnReport_WhenExists()
    {
        var reportId = Guid.NewGuid();
        var report = new Report { Id = reportId, Status = ReportStatus.Pending, DecorationId = "1" };
        _dbContext.Reports.Add(report);
        await _dbContext.SaveChangesAsync();
        
        var result = await _repository.FindReportAsync(reportId);
        
        Assert.NotNull(result);
        Assert.Equal(result, report);
    }

    [Fact]
    public async Task FindReportAsync_ShouldReturnNull_WhenNotExist()
    {
        var reportId = Guid.NewGuid();
        
        var result = await _repository.FindReportAsync(reportId);
        
        Assert.Null(result);
    }

    [Fact]
    public async Task FirstOrDefaultReportAsync_ShouldReturnReport_WhenExists()
    {
        var reportId = Guid.NewGuid();
        var report = new Report { Id = reportId, Status = ReportStatus.Pending, DecorationId = "1"};
        _dbContext.Reports.Add(report);
        await _dbContext.SaveChangesAsync();
        
        var result = await _repository.FirstOrDefaultAsync(reportId, CancellationToken.None);
        
        Assert.NotNull(result);
        Assert.Equal(result, report);
        
    }
    
    [Fact]
    public async Task FirstOrDefaultReportAsync_ShouldReturnReport_WhenNonExists()
    {
        var reportId = Guid.NewGuid();
        
        var result = await _repository.FirstOrDefaultAsync(reportId, CancellationToken.None);
        
        Assert.Null(result);
    }
    
    [Fact]
    public async Task Update_ShouldModifyReport_WhenExists()
    {
        var reportId = Guid.NewGuid();
        var report = new Report { Id = reportId, Status = ReportStatus.Pending, DecorationId = "1" };
        _dbContext.Reports.Add(report);
        await _dbContext.SaveChangesAsync();
        
        var result = await _repository.FindReportAsync(reportId);
        result!.Status = ReportStatus.Completed;
        await _repository.UpdateAsync(result, CancellationToken.None);
        
        var result2 = await _repository.FindReportAsync(reportId);
        Assert.NotNull(result2);
        Assert.Equal(ReportStatus.Completed, result2.Status);   
    }
    
    
    public void Dispose()
    {
        _dbContext.Dispose();
    }
}