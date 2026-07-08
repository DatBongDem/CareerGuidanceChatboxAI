using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities
{
    public class OperationalExpense
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty; // AI API & Infrastructure, Personnel, Marketing, Operational, Miscellaneous

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
