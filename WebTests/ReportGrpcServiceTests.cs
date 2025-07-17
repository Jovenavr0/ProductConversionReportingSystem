using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using ReportServiceApp;
using Web.Services;
using Xunit;
using Assert = Xunit.Assert;

namespace WebTests;

public class ReportGrpcServiceTests
{
    private readonly Mock<IOutboxService> _outboxService = new();
    private readonly Mock<IBillingService> _billingService = new();
    private readonly Mock<IReportRequestService> _reportRequestService = new();
    private readonly ReportGrpcService _service;

    public ReportGrpcServiceTests()
    {
        _service = new ReportGrpcService(_billingService.Object,  _outboxService.Object, _reportRequestService.Object);
    }

    [Fact]
    public async Task RequestReport_ShouldReturnReport()
    {
        var generatedReportId = "generatedReportId";
        var request = new ReportRequest
        {
            ProductId = 1,
            StartGap = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-7)),
            EndGap = Timestamp.FromDateTime(DateTime.UtcNow),
            UserId = "userId",
            DecorationId = "decorationId"
        };

        _outboxService.Setup(o => o.AddReportRequestAsync(It.IsAny<ReportRequestDto>()))
            .ReturnsAsync(generatedReportId);
        
        var response = await _service.RequestReport(request, new TestServerCallContext());
        
        Assert.Equal(generatedReportId, response.ReportId);
        Assert.Equal(ReportStatus.Pending, response.Status);
        _billingService.Verify(b => b.InitializeReportRequest(request.UserId, generatedReportId), Times.Once);
    }

    private class TestServerCallContext : ServerCallContext
    {
        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        {
            throw new NotImplementedException();
        }

        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
        {
            throw new NotImplementedException();
        }

        protected override string MethodCore { get; }
        protected override string HostCore { get; }
        protected override string PeerCore { get; }
        protected override DateTime DeadlineCore { get; }
        protected override Metadata RequestHeadersCore { get; }
        protected override CancellationToken CancellationTokenCore { get; }
        protected override Metadata ResponseTrailersCore { get; }
        protected override Status StatusCore { get; set; }
        protected override WriteOptions? WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore { get; }
    }
    
}