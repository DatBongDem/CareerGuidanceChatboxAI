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
        public List<RecommendedUniversityDto> Top3Universities { get; set; } = new List<RecommendedUniversityDto>();
        public List<RecommendedUniversityDto> Next5Universities { get; set; } = new List<RecommendedUniversityDto>();
    }

    public class RecommendedUniversityDto
    {
        public Guid UniversityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public double Ranking { get; set; }
        public string? Avatar { get; set; }
        public int MatchPercentage { get; set; }
        public List<MajorDto> SuitableMajors { get; set; } = new List<MajorDto>();
    }

    public class MajorDto
    {
        public Guid MajorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
