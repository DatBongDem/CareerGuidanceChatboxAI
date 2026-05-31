using BusinessLogic.DTOs.ChatAI.QuestionCategory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IQuestionCategoryService
    {
        Task<IEnumerable<QuestionCategoryDto>> GetAllAsync();
        Task<QuestionCategoryDto?> GetByIdAsync(Guid id);
        Task<QuestionCategoryDto> CreateAsync(CreateQuestionCategoryDto createDto);
        Task<bool> UpdateAsync(Guid id, UpdateQuestionCategoryDto updateDto);
        Task<bool> DeleteAsync(Guid id);
    }
}
