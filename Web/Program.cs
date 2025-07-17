using Application.Interfaces;
using Application.Services;
using Application.Settings;
using Application.Workers;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Messaging;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Web.Interceptors;
using Web.Services;

namespace Web;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(80); });
        builder.Services.AddSingleton<ExceptionInterceptor>();
        
        builder.Services.AddGrpc(options =>
        {
            options.Interceptors.Add<ExceptionInterceptor>();
            options.EnableDetailedErrors = builder.Environment.IsDevelopment();
        });
        builder.Services.AddMemoryCache();
        builder.Services.AddDbContext<AppDbContext>(options => 
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        
        builder.Services.Configure<BillingSettings>(builder.Configuration.GetSection("BillingSettings"));
        builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("CacheSettings"));
        builder.Services.Configure<BusSettings>(builder.Configuration.GetSection("BusSettings"));
        
        builder.Services.AddScoped<IViewRepository, ViewRepository>();
        builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
        builder.Services.AddScoped<IReportRepository, ReportRepository>();
        builder.Services.AddScoped<IBillingRepository, BillingRepository>();
        builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
        
        builder.Services.AddScoped<IOutboxService, OutboxService>();
        builder.Services.AddScoped<IBillingService, BillingService>();
        builder.Services.AddScoped<IReportRequestService, ReportRequestService>();
        builder.Services.AddScoped<IReportProcessorService, ReportProcessorService>();
        builder.Services.AddScoped<IReportDecorationService, ReportDecorationService>();
        
        builder.Services.AddSingleton<IMessageConsumer, KafkaMessageConsumer>();
        builder.Services.AddSingleton<IMessageProducer, KafkaMessageProducer>();

        builder.Services.AddHostedService<ReportWorker>();
        builder.Services.AddHostedService<OutboxWorker>();
        
        var app = builder.Build();
        app.MapGrpcService<ReportGrpcService>();
        app.Run();

    }
}