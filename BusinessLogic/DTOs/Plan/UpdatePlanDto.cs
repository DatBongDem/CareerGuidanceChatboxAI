using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Plan

{
    public class UpdatePlanDto
    {
     
        [Range(0, (double)decimal.MaxValue)]
        public decimal? Price { get; set; }
    }
}