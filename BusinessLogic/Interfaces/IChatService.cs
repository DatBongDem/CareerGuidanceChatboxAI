using BusinessLogic.DTOs.ChatAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IChatService
    {
        Task<string> AskAIAsync(Guid userId, string question);
        Task<GuidedChatResponse> ContinueGuidedChatAsync(Guid userId, Guid? sessionId, string? userMessage);
        Task<IEnumerable<ChatAiSessionDto>> GetUserChatSessionsAsync(Guid userId);
        Task<ChatAiSessionDetailDto?> GetChatSessionDetailAsync(Guid userId, Guid sessionId);
        Task<bool> DeleteChatSessionAsync(Guid userId, Guid sessionId);
    }
}
