using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using Assert = Xunit.Assert;

namespace InfrastructureTests;

public class OutboxRepositoryTests : IDisposable
{
    private readonly IOutboxRepository _repository;
    private readonly AppDbContext _dbContext;

    public OutboxRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(databaseName: "TestDb")
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options;
        
        _dbContext = new AppDbContext(options); 
        _repository = new OutboxRepository(_dbContext);
    }

    [Fact]
    public async Task GetOutboxMessagesAsync_ShouldReturnCorrectMessages()
    {
        const int messageCount = 2;
        var message1 = new OutboxMessage { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow.AddHours(-2), EventType = "", Payload = "test" };
        var message2 = new OutboxMessage { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow.AddHours(-1), EventType = "", Payload = "test" };
        var message3 = new OutboxMessage { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, EventType = "", Payload = "test" };
        await _dbContext.OutboxMessages.AddRangeAsync(new List<OutboxMessage> { message1, message2, message3 });
        await _dbContext.SaveChangesAsync();
        
        var result = await _repository.GetOutboxMessagesAsync(messageCount, CancellationToken.None);
        Assert.Contains(message1, result);
        Assert.Contains(message2, result);
    }
    
    [Fact]
    public async Task Update_ShouldModifyReport_WhenExists()
    {
        var message = new OutboxMessage { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow.AddHours(-2), EventType = "", Payload = "test" };
        _dbContext.OutboxMessages.Add(message);
        await _dbContext.SaveChangesAsync();
        
        var result = await _repository.GetOutboxMessagesAsync(1, CancellationToken.None);
        var message1 = result[0];
        message1.Payload = "updated";
        await _repository.UpdateAsync(message1, CancellationToken.None);
        
        var result2 = await _repository.GetOutboxMessagesAsync(1, CancellationToken.None);
        var message2 = result2[0];
        Assert.NotNull(message2);
        Assert.Equal("updated", message2.Payload);   
    }
    
    public void Dispose()
    {
        _dbContext.Dispose();
    }
}