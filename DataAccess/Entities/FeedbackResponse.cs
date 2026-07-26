using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities
{
    [Table("FeedbackResponses")]
    public class FeedbackResponse
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public string? UserEmail { get; set; }

        public string? UserFullName { get; set; }

        [Required]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public ICollection<FeedbackAnswer> Answers { get; set; } = new List<FeedbackAnswer>();
    }
}
