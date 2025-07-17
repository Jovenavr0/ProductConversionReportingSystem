using Application.Settings;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Workers;

public class ReportWorker (IServiceProvider serviceProvider, ILogger<ReportWorker> logger, IOptions<BusSettings> busSettings) : BackgroundService
{
    private readonly string _topicName = busSettings.Value.Topics.ReportRequests;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var messageConsumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer>();
                var processor = scope.ServiceProvider.GetRequiredService<IReportProcessorService>();

                await messageConsumer.ConsumeAsync(_topicName,
                    async (message) => await processor.ProcessMessageAsync(message, stoppingToken),
                    stoppingToken
                );
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("ReportWorker is stopping.");
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "Not all services are transferred to scope in ReportWorker");
                await Task.Delay(10000, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ReportWorker");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}