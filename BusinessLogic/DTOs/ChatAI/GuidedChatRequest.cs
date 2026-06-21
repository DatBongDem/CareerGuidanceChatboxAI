using System;

namespace BusinessLogic.DTOs.ChatAI
{
    public class GuidedChatRequest
    {
        public Guid? SessionId { get; set; }
        public string? Message { get; set; }
    }
}
