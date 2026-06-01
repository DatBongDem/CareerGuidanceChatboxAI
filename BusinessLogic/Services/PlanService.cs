using AutoMapper;
using BusinessLogic.DTOs.Plan;
using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;
using DataAccess.Shares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class PlanService : IPlanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PlanService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PlanDto>> GetAllPlans()
        {
            var plans = await _unitOfWork.PlanRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<PlanDto>>(plans);
        }

        public async Task<PlanDto> GetPlanById(Guid id)
        {
            var plan = await _unitOfWork.PlanRepository.GetByIdAsync(id);
            return _mapper.Map<PlanDto>(plan);
        }

        //public async Task<PlanDto> CreatePlan(CreatePlanDto createPlanDto)
        //{
        //    var plan = _mapper.Map<Plan>(createPlanDto);
        //    await _unitOfWork.PlanRepository.AddAsync(plan);
        //    await _unitOfWork.SaveAsync();
        //    return _mapper.Map<PlanDto>(plan);
        //}

        public async Task UpdatePlan(Guid id, UpdatePlanDto updatePlanDto)
        {
            var plan = await _unitOfWork.PlanRepository.GetByIdAsync(id);
            if (plan == null)
            {
                return;
            }

            _mapper.Map(updatePlanDto, plan);
            await _unitOfWork.PlanRepository.UpdateAsync(plan);
            await _unitOfWork.SaveAsync();
        }

        //public async Task DeletePlan(int id)
        //{
        //    var plan = await _unitOfWork.PlanRepository.GetByIdAsync(id);
        //    if (plan == null)
        //    {
        //        return;
        //    }
        //    await _unitOfWork.PlanRepository.DeleteAsync(id);
        //    await _unitOfWork.SaveAsync();
        //}

        //public async Task<IEnumerable<PlanHistoryDto>> GetPlanHistoryByUserIdAsync(Guid userId)
        //{
        //    var planHistories = await _unitOfWork.PlanHistoryRepository.GetAsync(
        //        filter: h => h.UserId == userId,
        //        orderBy: q => q.OrderByDescending(h => h.TransactionDate)
        //    );
        //    return _mapper.Map<IEnumerable<PlanHistoryDto>>(planHistories);
        //}

        //public async Task<PlanHistoryDto> RegisterVipPlanAsync(Guid userId)
        //{
        //    var activePlans = await _unitOfWork.PlanHistoryRepository.GetAsync(
        //        filter: h => h.UserId == userId && h.Expiry > DateTime.UtcNow
        //    );

        //    if (activePlans.Any())
        //    {
        //        throw new InvalidOperationException("User already has an active plan.");
        //    }

        //    var vipPlan = await _unitOfWork.PlanRepository.GetPlanByNameAsync("PRO");
        //    if (vipPlan == null)
        //    {
        //        throw new InvalidOperationException("PRO plan not found.");
        //    }

        //    var transactionDate = DateTime.UtcNow;
        //    var newPlanHistory = new PlanHistory
        //    {
        //        UserId = userId,
        //        Price = vipPlan.Price,
        //        TransactionDate = transactionDate,
        //        Method = "bank",
        //        NamePlan = "PRO",
        //        Expiry = transactionDate.AddDays(30)
        //    };

        //    await _unitOfWork.PlanHistoryRepository.AddAsync(newPlanHistory);
        //    await _unitOfWork.SaveAsync();

        //    return _mapper.Map<PlanHistoryDto>(newPlanHistory);
        //}
    }
}
