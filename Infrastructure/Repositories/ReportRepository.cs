using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ReportRepository(AppDbContext dbContext) : IReportRepository
{
    public async Task<Report?> FindReportAsync(Guid reportId)
    {
        return await dbContext.Reports.FindAsync(reportId);
    }

    public async Task<Report?> FirstOrDefaultAsync(Guid reportId, CancellationToken cancellationToken)
    {
        return await dbContext.Reports
            .FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);
    }
    
    public async Task UpdateAsync(Report report, CancellationToken cancellationToken)
    {
        dbContext.Reports.Update(report);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}