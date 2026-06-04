using BusinessLogic.DTOs.ChatAI.Question;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IQuestionService
    {
        Task<IEnumerable<QuestionDto>> GetAllAsync();
        Task<QuestionDto?> GetByIdAsync(Guid id);
        Task<QuestionDto> CreateAsync(CreateQuestionDto createDto);
        Task<bool> UpdateAsync(Guid id, UpdateQuestionDto updateDto);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<QuestionDto>> GetByCategoryIdAsync(Guid categoryId);
    }
}
