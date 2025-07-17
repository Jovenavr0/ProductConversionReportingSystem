namespace Domain.Records;

public record MessageBus(Guid ReportId, long ProductId, DateTime StartGap, DateTime EndGap);