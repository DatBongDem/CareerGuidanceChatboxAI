using BusinessLogic.DTOs.ChatAI;
using System;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IUserAiSummaryService
    {
        Task<UserAiSummaryResponseDto> EvaluateOverallAsync(Guid userId);
        Task<UserAiSummaryResponseDto?> GetOverallSummaryAsync(Guid userId);
        Task<UserAiSummaryResponseDto> EvaluateChatAiOverallAsync(Guid userId);
    }
}
