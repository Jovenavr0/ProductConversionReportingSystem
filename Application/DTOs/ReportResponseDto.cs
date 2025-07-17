using Domain.Enums;

namespace Application.DTOs;

public record ReportResponseDto(string ReportId, ReportStatus ReportStatus);