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
        Task UpdatePlan(Guid id, UpdatePlanDto updatePlanDto);
     
        //Task<IEnumerable<PlanHistoryDto>> GetPlanHistoryByUserIdAsync(Guid userId);
        //Task<PlanHistoryDto> RegisterVipPlanAsync(Guid userId);
    }
}


