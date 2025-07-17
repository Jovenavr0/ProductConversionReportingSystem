using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using Assert = Xunit.Assert;

namespace InfrastructureTests;

public class ViewRepositoryTests : IDisposable
{
    private readonly IViewRepository _repository;
    private readonly AppDbContext _dbContext;

    public ViewRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(databaseName: "TestDb")
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options;
        
        _dbContext = new AppDbContext(options); 
        _repository = new ViewRepository(_dbContext);
        
        _dbContext.Views.AddRange(new View { ProductId = 1, Timestamp = DateTime.UtcNow.AddHours(-2) },
            new View { ProductId = 1, Timestamp = DateTime.UtcNow.AddHours(-1) },
            new View { ProductId = 1, Timestamp = DateTime.UtcNow },
            new View { ProductId = 2, Timestamp = DateTime.UtcNow }
        );
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetViewsCountInGapAsync_ShouldReturnCorrectCount()
    {
        var productId = 1L;
        var start = DateTime.UtcNow.AddHours(-3);
        var end = DateTime.UtcNow;
        
        var count = await _repository.GetViewsCountInGapAsync(productId, start, end, CancellationToken.None);
        
        Assert.Equal(3, count);
    }
    
    public void Dispose()
    {
        _dbContext.Dispose();
    }
}