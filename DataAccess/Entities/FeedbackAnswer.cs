using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities
{
    [Table("FeedbackAnswers")]
    public class FeedbackAnswer
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ResponseId { get; set; }

        [ForeignKey("ResponseId")]
        public FeedbackResponse? Response { get; set; }

        [Required]
        public Guid QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public FeedbackQuestion? Question { get; set; }

        [Required]
        public string AnswerText { get; set; } = string.Empty;
    }
}
