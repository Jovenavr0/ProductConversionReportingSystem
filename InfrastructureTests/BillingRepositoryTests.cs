using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using Assert = Xunit.Assert;

namespace InfrastructureTests;

public class BillingRepositoryTests : IDisposable
{
    private readonly IBillingRepository _repository;
    private readonly AppDbContext _dbContext;

    public BillingRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(databaseName: "TestDb")
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options;
        
        _dbContext = new AppDbContext(options); 
        _repository = new BillingRepository(_dbContext);
    }

    [Fact]
    public async Task AddBillingOperationAsync_ShouldAddInDb()
    {
        var id = Guid.NewGuid();
        var operation = new BillingOperation { Id = id, Descrition = "", OperationType = "GetReport", UserId = "userId" };
        await _repository.AddOperationAsync(operation);

        var operation1 = await _dbContext.BillingOperations.FindAsync(id);
        Assert.NotNull(operation1);
        Assert.Equal(id, operation1.Id);
    }

    [Fact]
    public async Task GetReportAsync_ShouldReturnOperation_WhenExist()
    {
        var id = Guid.NewGuid();
        var report = new BillingReport { Id = id, UserId = "userId", LastPaymentTime = DateTime.Now };
        await _dbContext.BillingReports.AddAsync(report);
        await _dbContext.SaveChangesAsync();
        
        var result = _repository.GetReportByUserId("userId");
        Assert.NotNull(result);
        Assert.Equal(result, report);
    }
    
    [Fact]
    public void GetReportAsync_ShouldReturnNull_WhenNonExist()
    {
        var result = _repository.GetReportByUserId("userId");
        Assert.Null(result);
    }
    
    [Fact]
    public async Task Update_ShouldModifyReport_WhenExists()
    {
        var id = Guid.NewGuid();
        var report = new BillingReport { Id = id, UserId = "userId", LastPaymentTime = DateTime.Now };
        await _dbContext.BillingReports.AddAsync(report);
        await _dbContext.SaveChangesAsync();
        
        var result = _repository.GetReportByUserId("userId");
        result!.UserId = "updatedUserId";
        _dbContext.BillingReports.Update(result);
        await _dbContext.SaveChangesAsync();
        
        var result2 = _repository.GetReportByUserId("updatedUserId");
        Assert.NotNull(result2);
    }
    
    
    public void Dispose()
    {
        _dbContext.Dispose();
    }
}