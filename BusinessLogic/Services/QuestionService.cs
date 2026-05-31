using AutoMapper;
using BusinessLogic.DTOs.ChatAI.Question;
using BusinessLogic.Interfaces;
using DataAccess.Entities.ChatAI;
using DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public QuestionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<QuestionDto> CreateAsync(CreateQuestionDto createDto)
        {
            var categoryExists = await _unitOfWork.QuestionCategoryRepository.GetByIdAsync(createDto.CategoryId);
            if (categoryExists == null)
            {
                throw new Exception($"QuestionCategory with ID {createDto.CategoryId} not found.");
            }

            var question = _mapper.Map<Question>(createDto);
            
            await _unitOfWork.QuestionRepository.AddAsync(question);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<QuestionDto>(question);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var question = await _unitOfWork.QuestionRepository.GetByIdAsync(id);
            if (question == null)
            {
                return false;
            }

            await _unitOfWork.QuestionRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<IEnumerable<QuestionDto>> GetAllAsync()
        {
            var questions = await _unitOfWork.QuestionRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<QuestionDto>>(questions);
        }

        public async Task<QuestionDto?> GetByIdAsync(Guid id)
        {
            var question = await _unitOfWork.QuestionRepository.GetByIdAsync(id);
            return _mapper.Map<QuestionDto>(question);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateQuestionDto updateDto)
        {
            var question = await _unitOfWork.QuestionRepository.GetByIdAsync(id);
            if (question == null)
            {
                return false;
            }

            var categoryExists = await _unitOfWork.QuestionCategoryRepository.GetByIdAsync(updateDto.CategoryId);
            if (categoryExists == null)
            {
                throw new Exception($"QuestionCategory with ID {updateDto.CategoryId} not found.");
            }

            _mapper.Map(updateDto, question);
            await _unitOfWork.QuestionRepository.UpdateAsync(question);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}
