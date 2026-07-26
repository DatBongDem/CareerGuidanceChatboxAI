using BusinessLogic.DTOs.Feedback;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IFeedbackService
    {
        // Feedback Questions CRUD (Admin)
        Task<IEnumerable<FeedbackQuestionDto>> GetAllQuestionsAsync();
        Task<IEnumerable<FeedbackQuestionDto>> GetActiveQuestionsAsync();
        Task<FeedbackQuestionDto> GetQuestionByIdAsync(Guid id);
        Task<FeedbackQuestionDto> CreateQuestionAsync(CreateFeedbackQuestionDto dto);
        Task<FeedbackQuestionDto> UpdateQuestionAsync(Guid id, CreateFeedbackQuestionDto dto);
        Task DeleteQuestionAsync(Guid id);

        // Feedback Responses (Users & Admin)
        Task<bool> SubmitFeedbackAsync(SubmitFeedbackDto dto, Guid? userId);
        Task<IEnumerable<FeedbackResponseDto>> GetAllFeedbacksAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<FeedbackResponseDto?> GetFeedbackByIdAsync(Guid id);
        Task<byte[]> ExportFeedbacksToExcelAsync(DateTime startDate, DateTime endDate);
    }
}
