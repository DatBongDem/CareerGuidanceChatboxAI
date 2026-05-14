using System; // Added for Guid
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Plan; // Ensure this is present

namespace BusinessLogic.Interfaces
{
    public interface IPlanService
    {
        Task<IEnumerable<PlanDto>> GetAllPlans();
        Task<PlanDto> GetPlanById(Guid id);
        Task<PlanDto> CreatePlan(CreatePlanDto createPlanDto);
        Task UpdatePlan(Guid id, UpdatePlanDto updatePlanDto);
        Task DeletePlan(Guid id);
    }
}
