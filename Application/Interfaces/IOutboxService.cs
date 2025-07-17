using Application.DTOs;

namespace Application.Interfaces;

public interface IOutboxService
{
    Task<string> AddReportRequestAsync(ReportRequestDto reportDto);
}