using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities.ChatAI
{
    public class ChatAiSummary
    {
        [Key]
        public Guid Id { get; set; }

        public Guid SessionId { get; set; }

        public string SummaryText { get; set; } = string.Empty;

        public string Recommendations { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public ChatAiSession? Session { get; set; }
    }
}
