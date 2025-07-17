namespace Application.Settings;

public class BillingSettings
{
    public decimal ReportCost { get; set; } = 500.00m;
    public int FreePeriodHours { get; set; } = 24;
}