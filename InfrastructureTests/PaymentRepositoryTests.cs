using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using Assert = Xunit.Assert;

namespace InfrastructureTests;

public class PaymentRepositoryTests : IDisposable
{
    private readonly IPaymentRepository _repository;
    private readonly AppDbContext _dbContext;

    public PaymentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(databaseName: "TestDb")
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options;
        
        _dbContext = new AppDbContext(options); 
        _repository = new PaymentRepository(_dbContext);
        
        _dbContext.Payments.AddRange(new Payment { ProductId = 1, Timestamp = DateTime.UtcNow.AddHours(-2) },
            new Payment { ProductId = 1, Timestamp = DateTime.UtcNow.AddHours(-1) },
            new Payment { ProductId = 1, Timestamp = DateTime.UtcNow },
            new Payment { ProductId = 2, Timestamp = DateTime.UtcNow }
        );
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetViewsCountInGapAsync_ShouldReturnCorrectCount()
    {
        var productId = 1L;
        var start = DateTime.UtcNow.AddHours(-3);
        var end = DateTime.UtcNow;
        
        var payments = await _repository.GetPaymentsCountInGapAsync(productId, start, end, CancellationToken.None);
        
        Assert.Equal(3, payments);
    }
    
    public void Dispose()
    {
        _dbContext.Dispose();
    }
}