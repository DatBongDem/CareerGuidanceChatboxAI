using AutoMapper;
using BusinessLogic.DTOs.ChatAI.QuestionCategory;
using BusinessLogic.Interfaces;
using DataAccess.Entities.ChatAI;
using DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class QuestionCategoryService : IQuestionCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public QuestionCategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<QuestionCategoryDto> CreateAsync(CreateQuestionCategoryDto createDto)
        {
            var category = _mapper.Map<QuestionCategory>(createDto);
            
            await _unitOfWork.QuestionCategoryRepository.AddAsync(category);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<QuestionCategoryDto>(category);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var category = await _unitOfWork.QuestionCategoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return false;
            }

            await _unitOfWork.QuestionCategoryRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<IEnumerable<QuestionCategoryDto>> GetAllAsync()
        {
            var categories = await _unitOfWork.QuestionCategoryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<QuestionCategoryDto>>(categories);
        }

        public async Task<QuestionCategoryDto?> GetByIdAsync(Guid id)
        {
            var category = await _unitOfWork.QuestionCategoryRepository.GetByIdAsync(id);
            return _mapper.Map<QuestionCategoryDto>(category);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateQuestionCategoryDto updateDto)
        {
            var category = await _unitOfWork.QuestionCategoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return false;
            }

            _mapper.Map(updateDto, category);
            await _unitOfWork.QuestionCategoryRepository.UpdateAsync(category);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}
