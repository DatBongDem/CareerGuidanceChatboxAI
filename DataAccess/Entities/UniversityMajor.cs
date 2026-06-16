using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities
{
    public class UniversityMajor
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("University")]
        public Guid UniversityId { get; set; }

        [ForeignKey("Major")]
        public Guid MajorId { get; set; }

        public double? CutoffScore { get; set; }
        public int? Quota { get; set; }
        public int? Year { get; set; }

        public double? Tuition { get; set; }
        public string Currency { get; set; } = "VND";

        public int? DurationYears { get; set; }
        public string? DegreeType { get; set; }
        public string? Language { get; set; }

        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public University? University { get; set; }
        public Major? Major { get; set; }
    }
}