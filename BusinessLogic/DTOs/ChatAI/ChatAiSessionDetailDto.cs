using System;
using System.Collections.Generic;

namespace BusinessLogic.DTOs.ChatAI
{
    public class ChatAiSessionDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ChatAiSummaryResponseDto? Summary { get; set; }
        public List<ChatAiMessageDto> ChatHistory { get; set; } = new List<ChatAiMessageDto>();
    }

    public class ChatAiMessageDto
    {
        public Guid QuestionId { get; set; }
        public string QuestionContent { get; set; } = string.Empty;
        public string UserAnswer { get; set; } = string.Empty;
        public string? Evaluation { get; set; }
        public DateTime AnsweredAt { get; set; }
    }
}
