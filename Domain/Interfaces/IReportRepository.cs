using Domain.Entities;

namespace Domain.Interfaces;

public interface IReportRepository
{
    Task<Report?> FindReportAsync(Guid reportId);

    Task<Report?> FirstOrDefaultAsync(Guid reportId, CancellationToken cancellationToken);

    public Task UpdateAsync(Report report, CancellationToken cancellationToken);
}