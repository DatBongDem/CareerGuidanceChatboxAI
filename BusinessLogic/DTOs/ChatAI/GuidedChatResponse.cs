using System;

namespace BusinessLogic.DTOs.ChatAI
{
    public class GuidedChatResponse
    {
        public Guid SessionId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Evaluation { get; set; } = string.Empty;
        public bool HasEnoughInfo { get; set; }
        public ChatAiSummaryResponseDto? Summary { get; set; }
        public string? NextQuestionContent { get; set; }
    }
}
