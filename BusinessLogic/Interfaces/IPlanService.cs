using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.DTOs.Plan;

namespace BusinessLogic.Interfaces
{
    public interface IPlanService
    {
        Task<IEnumerable<PlanDto>> GetAllPlans();
        Task<PlanDto> GetPlanById(Guid id);
     // Task<PlanDto> CreatePlan(CreatePlanDto createPlanDto);
        Task UpdatePlan(Guid id, UpdatePlanDto updatePlanDto);
     // Task DeletePlan(Guid id);
        Task<IEnumerable<PlanHistoryDto>> GetPlanHistoryByUserIdAsync(Guid userId);
        Task<PlanHistoryDto> RegisterVipPlanAsync(Guid userId);
    }
}


