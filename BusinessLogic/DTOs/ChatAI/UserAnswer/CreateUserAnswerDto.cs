using System;

namespace BusinessLogic.DTOs.ChatAI.UserAnswer
{
    public class CreateUserAnswerDto
    {
        public Guid QuestionId { get; set; }
        public string Answer { get; set; } = string.Empty;
    }
}
