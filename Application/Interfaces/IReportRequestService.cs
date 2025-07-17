using Application.DTOs;

namespace Application.Interfaces;

public interface IReportRequestService
{
    public Task<ReportResponseDto> Execute(string reportId);
}