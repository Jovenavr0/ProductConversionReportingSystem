namespace Application.DTOs;

public record ReportRequestDto(
    long ProductId,
    DateTime StartGap,
    DateTime EndGap,
    string UserId,
    string DecorationId
);