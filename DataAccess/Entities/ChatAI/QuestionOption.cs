using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities.ChatAI
{
    public class QuestionOption
    {
        public Guid Id { get; set; }

        public Guid QuestionId { get; set; }

        public string OptionCode { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public string? ScoreTag { get; set; }

        public Question Question { get; set; }
            = null!;
    }
}
