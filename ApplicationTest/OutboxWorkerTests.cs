using Application.Settings;
using Application.Workers;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace ApplicationTest;

public class OutboxWorkerTests
{
    private readonly Mock<ILogger<OutboxWorker>> _logger = new();
    private readonly Mock<IServiceProvider> _serviceProvider = new();
    private readonly Mock<IServiceScope> _scope = new();
    private readonly Mock<IOutboxRepository> _outboxRepository = new();
    private readonly Mock<IMessageProducer> _messageProducer = new();
    
    private readonly OutboxWorker _service;
    private readonly CancellationTokenSource _cancellationToken = new();
    private readonly Mock<IOptions<BusSettings>> _busSettingsMock = new();

    public OutboxWorkerTests()
    {
        SetupServiceProvider();
        _busSettingsMock.Setup(x => x.Value).Returns(new BusSettings
        {
            BootstrapServers = "kafka:9092",
            GroupId = "report-worker-group",
            AutoOffsetReset = "latest",
            EnableAutoCommit = false,
            Topics = new BusTopics(),
            BatchSize = 100
        });
        _service = new OutboxWorker( _logger.Object, _serviceProvider.Object, _busSettingsMock.Object );
    }

    private void SetupServiceProvider()
    {
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup( x => x.CreateScope() ).Returns( _scope.Object );
        _serviceProvider.Setup( x => x.GetService( typeof(IServiceScopeFactory) ) ).Returns( scopeFactoryMock.Object );
        _scope.Setup(x => x.ServiceProvider.GetService( typeof( IOutboxRepository ) ) ).Returns( _outboxRepository.Object );
        _scope.Setup(x => x.ServiceProvider.GetService( typeof( IMessageProducer ) ) ).Returns( _messageProducer.Object );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldProduceMessages()
    {
        var messages = new List<OutboxMessage>
        {
            new() { Id = Guid.NewGuid(), Payload = "payloadFirst" },
            new() { Id = Guid.NewGuid(), Payload = "payloadSecond" }
        };
        
        _outboxRepository.Setup(r => r.GetOutboxMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync( messages );

        _messageProducer.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>())).Returns( Task.CompletedTask );
        
        _outboxRepository.Setup(r => r.UpdateAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>())).Returns( Task.CompletedTask );
        
        _ = _service.StartAsync(_cancellationToken.Token);
        await Task.Delay(500);
        await _service.StopAsync(_cancellationToken.Token);
        
        _messageProducer.Verify( x => x.ProduceAsync( "report-requests", messages[0], It.IsAny<CancellationToken>() ), Times.Once() );
        _messageProducer.Verify( x => x.ProduceAsync( "report-requests", messages[1], It.IsAny<CancellationToken>() ), Times.Once() );

        Assert.True(messages[0].Processed);
        Assert.True(messages[1].Processed);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldHandleMessageProcessingError()
    {
        var message = new OutboxMessage { Id = Guid.NewGuid(), Payload = "payloadFirst" };
        var messages = new List<OutboxMessage> { message };
        
        _outboxRepository.Setup(r => r.GetOutboxMessagesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync( messages );

        _messageProducer.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>())).
            ThrowsAsync(new Exception( "message processing error" ));
        
        _ = _service.StartAsync(_cancellationToken.Token);
        await Task.Delay(500);
        await _service.StopAsync(_cancellationToken.Token);
        
        Assert.False(messages[0].Processed);
    }
    
}