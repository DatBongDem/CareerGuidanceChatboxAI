using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities.ChatAI
{
    public class AiEvaluation
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid CategoryId { get; set; }

        public string EvaluationText { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("CategoryId")]
        public QuestionCategory? Category { get; set; }
    }
}
