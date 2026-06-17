using System;

namespace DataAccess.Entities.ChatAI
{
    public class UserAiSummary
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string SummaryText { get; set; } = string.Empty;
        public string Top3UniversityIds { get; set; } = string.Empty;
        public string Next5UniversityIds { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
