using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities.ChatAI
{
    public class ChatAiAnswer
    {
        [Key]
        public Guid Id { get; set; }

        public Guid SessionId { get; set; }

        public Guid QuestionId { get; set; }

        public string Answer { get; set; } = string.Empty;

        public string? Evaluation { get; set; }

        public DateTime AnsweredAt { get; set; }

        // Navigation property
        public ChatAiSession? Session { get; set; }

        public Question? Question { get; set; }
    }
}
