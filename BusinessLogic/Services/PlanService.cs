using AutoMapper;
using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Plan;
using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;
using System; 
using System.Collections.Generic;
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

        public async Task<PlanDto> CreatePlan(CreatePlanDto createPlanDto)
        {
            var plan = _mapper.Map<Plan>(createPlanDto);
            await _unitOfWork.PlanRepository.AddAsync(plan);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<PlanDto>(plan);
        }

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

        public async Task DeletePlan(Guid id)
        {
            var plan = await _unitOfWork.PlanRepository.GetByIdAsync(id);
            if (plan == null)
            {
                return;
            }
            await _unitOfWork.PlanRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
        }
    }
}
