using System;

namespace BusinessLogic.DTOs.ChatAI.QuestionOption
{
    public class QuestionOptionDto
    {
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }
        public string OptionCode { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public string? ScoreTag { get; set; }
    }
}
