using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Feedback
{
    public class CreateFeedbackQuestionDto
    {
        [Required(ErrorMessage = "Nội dung câu hỏi là bắt buộc.")]
        public string QuestionText { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại câu hỏi là bắt buộc.")]
        public string QuestionType { get; set; } = "Text"; // Text, Rating, YesNo, MultipleChoice

        public string? Options { get; set; }

        public int Order { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }
}
