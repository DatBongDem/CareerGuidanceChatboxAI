using BusinessLogic.DTOs.Feedback;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class FeedbackQuestionsController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackQuestionsController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        // --- PUBLIC ENDPOINTS (USERS) ---

        [HttpGet("feedback-questions/active")]
        public async Task<IActionResult> GetActiveQuestions()
        {
            var data = await _feedbackService.GetActiveQuestionsAsync();
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách câu hỏi phản hồi thành công.",
                data
            });
        }

        // --- ADMIN ENDPOINTS (CRUD) ---

        [Authorize(Roles = "ADMIN")]
        [HttpGet("admin/feedback-questions")]
        public async Task<IActionResult> GetAllQuestions()
        {
            var data = await _feedbackService.GetAllQuestionsAsync();
            return Ok(new
            {
                success = true,
                message = "Lấy toàn bộ danh sách câu hỏi phản hồi thành công.",
                data
            });
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("admin/feedback-questions/{id}")]
        public async Task<IActionResult> GetQuestionById(Guid id)
        {
            try
            {
                var question = await _feedbackService.GetQuestionByIdAsync(id);
                return Ok(new
                {
                    success = true,
                    data = question
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("admin/feedback-questions")]
        public async Task<IActionResult> CreateQuestion([FromBody] CreateFeedbackQuestionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var question = await _feedbackService.CreateQuestionAsync(dto);
            return CreatedAtAction(nameof(GetQuestionById), new { id = question.Id }, new
            {
                success = true,
                message = "Tạo câu hỏi phản hồi thành công.",
                data = question
            });
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("admin/feedback-questions/{id}")]
        public async Task<IActionResult> UpdateQuestion(Guid id, [FromBody] CreateFeedbackQuestionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var question = await _feedbackService.UpdateQuestionAsync(id, dto);
                return Ok(new
                {
                    success = true,
                    message = "Cập nhật câu hỏi phản hồi thành công.",
                    data = question
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpDelete("admin/feedback-questions/{id}")]
        public async Task<IActionResult> DeleteQuestion(Guid id)
        {
            try
            {
                await _feedbackService.DeleteQuestionAsync(id);
                return Ok(new
                {
                    success = true,
                    message = "Xóa câu hỏi phản hồi thành công."
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
