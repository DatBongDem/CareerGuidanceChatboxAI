using System;
using System.Collections.Generic;

namespace BusinessLogic.DTOs.ChatAI
{
    public class ChatAiSummaryResponseDto
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public string SummaryText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<RecommendedUniversityDto> Recommendations { get; set; } = new List<RecommendedUniversityDto>();
    }
}
