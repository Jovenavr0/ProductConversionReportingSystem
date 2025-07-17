namespace Application.Settings;

public class BusSettings
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string GroupId { get; set; } = "report-worker-group";
    public string AutoOffsetReset { get; set; } = "Latest";
    public bool EnableAutoCommit { get; set; } = false;
    public int BatchSize { get; set; } = 100;
    public BusTopics Topics { get; set; } = new BusTopics();
    
}

public class BusTopics
{
    public string ReportRequests { get; set; } = "report-requests";
}