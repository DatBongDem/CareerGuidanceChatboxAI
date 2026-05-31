using DataAccess.Shares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities.ChatAI
{
    public class Question
    {
        public Guid Id { get; set; }

        public Guid CategoryId { get; set; }

        public string Content { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public bool AllowCustomAnswer { get; set; }

        public string IsActice { get; set; } = StatusEnum.Yes;

        public QuestionCategory Category { get; set; }
            = null!;
        
        public ICollection<QuestionOption> Options { get; set; }
            = new List<QuestionOption>();
    }
}
