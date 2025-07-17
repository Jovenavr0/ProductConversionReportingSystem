using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain.Entities;

public class Report
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public long ProductId { get; set; }
    
    [Required]
    public DateTime StartGap { get; set; }
    
    [Required]
    public DateTime EndGap { get; set; }
    
    [Required]
    public string DecorationId { get; set; }

    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    
    public double? Ratio { get; set; }
    
    public int? PaymentsCount { get; set; }
    
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
}