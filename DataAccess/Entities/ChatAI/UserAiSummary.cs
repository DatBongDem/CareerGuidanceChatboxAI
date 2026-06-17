using System;

namespace DataAccess.Entities.ChatAI
{
    public class UserAiSummary
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string SummaryText { get; set; } = string.Empty;
        public string Top3Recommendations { get; set; } = string.Empty;
        public string Next5Recommendations { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
