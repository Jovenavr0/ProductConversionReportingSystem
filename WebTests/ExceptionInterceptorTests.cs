using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using ReportServiceApp;
using Web.Interceptors;
using Xunit;
using Assert = Xunit.Assert;

namespace WebTests;

public class ExceptionInterceptorTests
{
    [Fact]
    public async Task ShouldHandleException()
    {
        var loggerMock = new Mock<ILogger<ExceptionInterceptor>>();
        var interceptor = new ExceptionInterceptor(loggerMock.Object);

        var request = new ReportRequest();
        var context = new Mock<ServerCallContext>().Object;
        var continuation = new Mock<UnaryServerMethod<ReportRequest, ReportResponse>>();
        
        continuation.Setup(c => c(request, context)).ThrowsAsync(new Exception());
        
        await Assert.ThrowsAsync<RpcException>(() => interceptor.UnaryServerHandler(request, context, continuation.Object));
    }
}