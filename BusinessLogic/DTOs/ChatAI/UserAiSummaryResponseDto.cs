using System;
using System.Collections.Generic;

namespace BusinessLogic.DTOs.ChatAI
{
    public class UserAiSummaryResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string SummaryText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<UniversityDto> Top3Universities { get; set; } = new List<UniversityDto>();
        public List<UniversityDto> Next5Universities { get; set; } = new List<UniversityDto>();
    }

    public class UniversityDto
    {
        public Guid UniversityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public double Ranking { get; set; }
        public string? Avatar { get; set; }
    }
}
