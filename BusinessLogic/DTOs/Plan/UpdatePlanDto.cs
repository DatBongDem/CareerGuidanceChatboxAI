using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs
{
    public class UpdatePlanDto
    {
     
        public string? Name { get; set; }

        
        public string? Description { get; set; }

        [Range(0, (double)decimal.MaxValue)]
        public decimal? Price { get; set; }
    }
}