using System;

namespace BusinessLogic.DTOs.ChatAI.Question
{
    public class QuestionDto
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool AllowCustomAnswer { get; set; }
        public string IsActice { get; set; }
    }
}
