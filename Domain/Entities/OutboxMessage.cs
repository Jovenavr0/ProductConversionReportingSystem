using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class OutboxMessage
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string EventType { get; set; } = null!;
    
    [Required]
    public string Payload { get; set; } = null!;
    
    public bool Processed { get; set; }
    public DateTime? ProcessedAt { get; set; }
}