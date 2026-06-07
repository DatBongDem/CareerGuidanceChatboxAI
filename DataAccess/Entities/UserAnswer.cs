using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities
{
    public class UserAnswer
    {
        [Key]
        public Guid UserAnswerId { get; set; }

        public Guid UserId { get; set; }

        public Guid QuestionId { get; set; }

        public string Answer { get; set; } = string.Empty;

        public DateTime AnsweredAt { get; set; }
    }
}
