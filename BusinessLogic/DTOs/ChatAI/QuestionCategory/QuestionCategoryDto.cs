using System;

namespace BusinessLogic.DTOs.ChatAI.QuestionCategory
{
    public class QuestionCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }
}
