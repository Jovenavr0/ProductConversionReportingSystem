using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Payment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    
    [Required]
    public long ProductId { get; set; }
    
    [Required]
    public DateTime Timestamp { get; set; }
    
    public decimal Amount { get; set; }
    
}