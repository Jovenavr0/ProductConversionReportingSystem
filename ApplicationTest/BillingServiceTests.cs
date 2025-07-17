using Application.Services;
using Application.Settings;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace ApplicationTest;

public class BillingServiceTests
{
    private readonly Mock<IBillingRepository> _mockRepo = new();
    private readonly Mock<IOptions<BillingSettings>> _billingSettingsMock = new();
    private readonly BillingService _service;

    public BillingServiceTests()
    {
        _billingSettingsMock.Setup(x => x.Value).Returns(new BillingSettings
        {
            ReportCost = 500.00m,
            FreePeriodHours = 24
        });
        _service = new BillingService(_mockRepo.Object, _billingSettingsMock.Object);
    }

    [Fact]
    public async Task InitializeReportRequest_ShouldAddOperation_WhenPaymentNeeded()
    {
        const string userId = "uesr1";
        var reportId = "report1";
        
        _mockRepo.Setup(r => r.GetReportByUserId(userId)).Returns((BillingReport)null);
        
        await _service.InitializeReportRequest(userId, reportId);
        
        _mockRepo.Verify(r => r.AddOperationAsync(It.Is<BillingOperation>(op =>
            op.UserId == userId &&
            op.Amount == 500.00m &&
            op.OperationType == "GenerationReport")), Times.Once
        );
        
        _mockRepo.Verify(r => r.AddOperationAsync(It.IsAny<BillingOperation>()), Times.Once);
    }

    [Fact]
    public async Task InitializeReportRequest_ShouldNotAddOperation_WithinFreePeriod()
    {
        var userId = "userId2";
        var reportId = Guid.NewGuid();
        var lastPayment = new BillingReport
        {
            Id = reportId,
            UserId = userId,
            LastPaymentTime = DateTime.UtcNow.AddHours(-12)
        };
        
        _mockRepo.Setup(r => r.GetReportByUserId(userId)).Returns(lastPayment);
        
        await _service.InitializeReportRequest(userId, reportId.ToString());
        
        _mockRepo.Verify(r => r.AddOperationAsync(It.IsAny<BillingOperation>()), Times.Never);
        _mockRepo.Verify(r => r.UpdateReportAsync(It.IsAny<BillingReport>()), Times.Never);
    }
    
}