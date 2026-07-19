using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Finance
{
    public class UpdateExpenseDto
    {
        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }
    }
}
