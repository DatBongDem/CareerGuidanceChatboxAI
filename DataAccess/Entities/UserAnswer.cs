using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities
{
    public class UserAnswer
    {
        [Key]
        public Guid UserAnswerId { get; set; }

        public Guid ProfileId { get; set; }

        public Guid QuestionId { get; set; }

        public Guid AnswerId { get; set; }
    }
}
