using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities
{
    [Table("FeedbackQuestions")]
    public class FeedbackQuestion
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        public string QuestionType { get; set; } = "Text"; // Text, Rating, YesNo, MultipleChoice

        public string? Options { get; set; } // Comma-separated options for MultipleChoice

        [Required]
        public int Order { get; set; } = 0;

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
