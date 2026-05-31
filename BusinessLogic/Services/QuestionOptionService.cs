using AutoMapper;
using BusinessLogic.DTOs.ChatAI.QuestionOption;
using BusinessLogic.Interfaces;
using DataAccess.Entities.ChatAI;
using DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class QuestionOptionService : IQuestionOptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public QuestionOptionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<QuestionOptionDto> CreateAsync(CreateQuestionOptionDto createDto)
        {
            var questionExists = await _unitOfWork.QuestionRepository.GetByIdAsync(createDto.QuestionId);
            if (questionExists == null)
            {
                throw new Exception($"Question with ID {createDto.QuestionId} not found.");
            }

            var option = _mapper.Map<QuestionOption>(createDto);
            
            await _unitOfWork.QuestionOptionRepository.AddAsync(option);
            await _unitOfWork.SaveAsync();

            return _mapper.Map<QuestionOptionDto>(option);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var option = await _unitOfWork.QuestionOptionRepository.GetByIdAsync(id);
            if (option == null)
            {
                return false;
            }

            await _unitOfWork.QuestionOptionRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<IEnumerable<QuestionOptionDto>> GetAllAsync()
        {
            var options = await _unitOfWork.QuestionOptionRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<QuestionOptionDto>>(options);
        }

        public async Task<QuestionOptionDto?> GetByIdAsync(Guid id)
        {
            var option = await _unitOfWork.QuestionOptionRepository.GetByIdAsync(id);
            return _mapper.Map<QuestionOptionDto>(option);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateQuestionOptionDto updateDto)
        {
            var option = await _unitOfWork.QuestionOptionRepository.GetByIdAsync(id);
            if (option == null)
            {
                return false;
            }

            var questionExists = await _unitOfWork.QuestionRepository.GetByIdAsync(updateDto.QuestionId);
            if (questionExists == null)
            {
                throw new Exception($"Question with ID {updateDto.QuestionId} not found.");
            }

            _mapper.Map(updateDto, option);
            await _unitOfWork.QuestionOptionRepository.UpdateAsync(option);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}
