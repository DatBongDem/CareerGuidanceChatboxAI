using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Entities.ChatAI
{
    public class ChatAiSession
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
