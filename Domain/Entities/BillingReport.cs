using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class BillingReport
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(255)]
    public string UserId { get; set; }
    
    [Required]
    public DateTime LastPaymentTime { get; set; }
}