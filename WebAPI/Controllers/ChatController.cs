using BusinessLogic.DTOs.ChatAI;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [Authorize]
        [HttpPost("ask")]
        public async Task<IActionResult> Ask(ChatRequest request)
        {
            var userId = Guid.Parse(
                User.FindFirst("UserId")!.Value);

            var answer = await _chatService.AskAIAsync(
                userId,
                request.Question);

            return Ok(new ChatResponse
            {
                Answer = answer
            });
        }

        [Authorize]
        [HttpPost("guided")]
        public async Task<IActionResult> Guided(GuidedChatRequest request)
        {
            var userId = Guid.Parse(
                User.FindFirst("UserId")!.Value);

            var result = await _chatService.ContinueGuidedChatAsync(
                userId,
                request.Message);

            return Ok(result);
        }

        [Authorize]
        [HttpDelete("guided")]
        public async Task<IActionResult> ResetGuidedChat()
        {
            var userId = Guid.Parse(
                User.FindFirst("UserId")!.Value);

            var success = await _chatService.ResetGuidedChatAsync(userId);

            if (!success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không tìm thấy dữ liệu chat hoặc không thể xóa lịch sử chat AI."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Đã xóa lịch sử chat AI thành công."
            });
        }
    }
}
