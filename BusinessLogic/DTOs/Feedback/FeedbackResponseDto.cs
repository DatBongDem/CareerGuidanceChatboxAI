using System;
using System.Collections.Generic;

namespace BusinessLogic.DTOs.Feedback
{
    public class FeedbackResponseDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? UserFullName { get; set; }
        public DateTime SubmittedAt { get; set; }
        public List<FeedbackAnswerDto> Answers { get; set; } = new List<FeedbackAnswerDto>();
    }

    public class FeedbackAnswerDto
    {
        public Guid QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "Text";
        public string AnswerText { get; set; } = string.Empty;
    }
}
