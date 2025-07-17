using System.Text.Json;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Services;

public class OutboxService(AppDbContext dbContext) : IOutboxService
{
    public async Task<string> AddReportRequestAsync(ReportRequestDto reportDto)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        var initialTime = DateTime.UtcNow;
        
        try
        {
            var report = new Report
            {
                Id = Guid.NewGuid(),
                ProductId = reportDto.ProductId,
                StartGap = reportDto.StartGap,
                EndGap = reportDto.EndGap,
                Status = ReportStatus.Pending,
                CreatedAt = initialTime,
                DecorationId = reportDto.DecorationId
            };
            
            dbContext.Reports.Add(report);
    
            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                CreatedAt = initialTime,
                EventType = "ReportRequested",
                Payload = JsonSerializer.Serialize(new
                {
                    report.Id,
                    report.ProductId,
                    report.StartGap,
                    report.EndGap
                }),
                Processed = false
            };
    
            dbContext.OutboxMessages.Add(outboxMessage);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return report.Id.ToString();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}