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
                request.SessionId,
                request.Message);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("guided/sessions")]
        public async Task<IActionResult> GetSessions()
        {
            var userId = Guid.Parse(
                User.FindFirst("UserId")!.Value);

            var result = await _chatService.GetUserChatSessionsAsync(userId);
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách phiên chat AI thành công.",
                data = result
            });
        }

        [Authorize]
        [HttpGet("guided/sessions/{sessionId}")]
        public async Task<IActionResult> GetSessionDetail(Guid sessionId)
        {
            var userId = Guid.Parse(
                User.FindFirst("UserId")!.Value);

            var result = await _chatService.GetChatSessionDetailAsync(userId, sessionId);
            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy chi tiết phiên chat hoặc phiên chat không thuộc về bạn."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Lấy chi tiết phiên chat AI thành công.",
                data = result
            });
        }

        [Authorize]
        [HttpDelete("guided/sessions/{sessionId}")]
        public async Task<IActionResult> DeleteSession(Guid sessionId)
        {
            var userId = Guid.Parse(
                User.FindFirst("UserId")!.Value);

            var success = await _chatService.DeleteChatSessionAsync(userId, sessionId);

            if (!success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không tìm thấy phiên chat hoặc không thể xóa."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Đã xóa phiên chat AI thành công."
            });
        }
    }
}
