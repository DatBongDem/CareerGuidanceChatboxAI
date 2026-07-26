using System;

namespace BusinessLogic.DTOs.Feedback
{
    public class FeedbackQuestionDto
    {
        public Guid Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "Text"; // Text, Rating, YesNo, MultipleChoice
        public string? Options { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
