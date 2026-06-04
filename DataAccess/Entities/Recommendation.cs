using System.ComponentModel.DataAnnotations;

public class Recommendation
{
    [Key]
    public Guid RecommendationId { get; set; }

    public Guid ProfileId { get; set; }

    public Guid MajorId { get; set; }

    public double MatchScore { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; }
}