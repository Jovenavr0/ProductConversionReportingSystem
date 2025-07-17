using Domain.Entities;

namespace Domain.Interfaces;

public interface IReportDecorationService
{
    void GenerateDecorationReport(Report report);
}