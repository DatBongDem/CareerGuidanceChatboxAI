using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities.ChatAI
{
    public class ChatHistory
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Question { get; set; } = null!;

        public string Answer { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
