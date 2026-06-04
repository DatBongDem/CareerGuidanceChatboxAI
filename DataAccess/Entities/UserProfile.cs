using System.ComponentModel.DataAnnotations;

public class UserProfile
{
    [Key]
    public Guid ProfileId { get; set; }

    public Guid UserId { get; set; }

    public double GPA { get; set; }

    public string? StrengthSubjects { get; set; }

    public string? Interests { get; set; }

    public string? Personality { get; set; }

    public string? CareerGoals { get; set; }
}
