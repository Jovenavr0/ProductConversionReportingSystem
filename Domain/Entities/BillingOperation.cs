using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class BillingOperation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string UserId { get; set; }
    
    [Required]
    public DateTime OperationTime { get; set; } = DateTime.UtcNow;
    
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }
    
    [Required]
    [StringLength(50)]
    public string OperationType { get; set; }
    
    public string Descrition { get; set; }
}