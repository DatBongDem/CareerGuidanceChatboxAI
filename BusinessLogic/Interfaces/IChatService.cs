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
        Task<GuidedChatResponse> ContinueGuidedChatAsync(Guid userId, string? userMessage);
    }
}
