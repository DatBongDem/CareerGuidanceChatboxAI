namespace BusinessLogic.DTOs.ChatAI
{
    public class GuidedChatResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Evaluation { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public UserAiSummaryResponseDto? Summary { get; set; }
    }
}
