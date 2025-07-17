using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Web.Interceptors;

public class ExceptionInterceptor(ILogger<ExceptionInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            logger.LogInformation("Start call. Method: {method}. Request: {request}", context.Method, request);
            var response = await continuation(request, context);
            logger.LogInformation("Finished call. Method: {method}. Response: {response}", context.Method, response);
            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in {method}", context.Method);
            throw new RpcException(new Status(StatusCode.Internal, "Internal Server Error"));
        }
    }
}