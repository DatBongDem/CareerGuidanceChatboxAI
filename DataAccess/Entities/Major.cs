using DataAccess.Entities;
using System.Text.Json.Serialization;

public class Major
{
    public Guid MajorId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    [JsonIgnore]
    public ICollection<UniversityMajor>? UniversityMajors { get; set; }

}