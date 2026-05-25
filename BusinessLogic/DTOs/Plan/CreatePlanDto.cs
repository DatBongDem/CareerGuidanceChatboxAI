using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Plan
{
    public class CreatePlanDto
    {
        [Required]    
        public string Name { get; set; }

        [Required]        
        public string Description { get; set; }

        [Required]
        [Range(0, (double)decimal.MaxValue)]
        public decimal Price { get; set; }
    }
}