using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities
{
    public class EduRegistration
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public string SchoolName { get; set; } = string.Empty;
        
        [Required]
        public string ContactName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Range(1, int.MaxValue)]
        public int StudentCount { get; set; }
        
        public string Notes { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        
        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Paid, Completed, Cancelled
        
        public Guid PlanId { get; set; }
        
        public string? TransactionCode { get; set; }
        
        public string? CheckoutUrl { get; set; }
        public string? Bin { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountName { get; set; }
        public string? PaymentDescription { get; set; }
        
        // Navigation properties
        [ForeignKey("PlanId")]
        public Plan? Plan { get; set; }
    }
}
