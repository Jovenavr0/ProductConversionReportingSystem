namespace Application.Settings;

public class CacheSettings
{
    public int ExpirationHoursPayload { get; set; } = 24;
    public double ExpirationHoursReports { get; set; } = 0.5;
}