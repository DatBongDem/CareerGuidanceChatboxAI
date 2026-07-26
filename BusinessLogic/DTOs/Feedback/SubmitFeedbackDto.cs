using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Feedback
{
    public class SubmitFeedbackDto
    {
        public string? UserEmail { get; set; }
        public string? UserFullName { get; set; }

        [Required(ErrorMessage = "Danh sách câu trả lời là bắt buộc.")]
        public List<SubmitFeedbackAnswerDto> Answers { get; set; } = new List<SubmitFeedbackAnswerDto>();
    }

    public class SubmitFeedbackAnswerDto
    {
        [Required(ErrorMessage = "QuestionId là bắt buộc.")]
        public Guid QuestionId { get; set; }

        [Required(ErrorMessage = "Nội dung câu trả lời là bắt buộc.")]
        public string AnswerText { get; set; } = string.Empty;
    }
}
