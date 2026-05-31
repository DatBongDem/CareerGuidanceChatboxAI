using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities.ChatAI
{
    public class QuestionCategory
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public ICollection<Question> Questions { get; set; }
            = new List<Question>();
    }
}
