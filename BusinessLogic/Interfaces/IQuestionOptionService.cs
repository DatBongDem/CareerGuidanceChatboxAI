using BusinessLogic.DTOs.ChatAI.QuestionOption;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IQuestionOptionService
    {
        Task<IEnumerable<QuestionOptionDto>> GetAllAsync();
        Task<QuestionOptionDto?> GetByIdAsync(Guid id);
        Task<QuestionOptionDto> CreateAsync(CreateQuestionOptionDto createDto);
        Task<bool> UpdateAsync(Guid id, UpdateQuestionOptionDto updateDto);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<QuestionOptionDto>> GetByQuestionIdAsync(Guid questionId);
    }
}
