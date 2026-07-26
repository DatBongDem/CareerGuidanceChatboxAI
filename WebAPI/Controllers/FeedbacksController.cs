using BusinessLogic.DTOs.Feedback;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class FeedbacksController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbacksController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        // --- PUBLIC ENDPOINT (USERS) ---

        [HttpPost("feedbacks")]
        public async Task<IActionResult> SubmitFeedback([FromBody] SubmitFeedbackDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Guid? userId = null;
            var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            try
            {
                var success = await _feedbackService.SubmitFeedbackAsync(dto, userId);
                return Ok(new
                {
                    success,
                    message = "Gửi phản hồi thành công. Cảm ơn ý kiến của bạn!"
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // --- ADMIN ENDPOINTS ---

        [Authorize(Roles = "ADMIN")]
        [HttpGet("admin/feedbacks")]
        public async Task<IActionResult> GetAllFeedbacks([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var data = await _feedbackService.GetAllFeedbacksAsync(startDate, endDate);
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách phản hồi thành công.",
                data
            });
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("admin/feedbacks/{id}")]
        public async Task<IActionResult> GetFeedbackById(Guid id)
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
            if (feedback == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy phản hồi này." });
            }

            return Ok(new
            {
                success = true,
                data = feedback
            });
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("admin/feedbacks/export")]
        public async Task<IActionResult> ExportFeedbacks([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest(new { success = false, message = "Ngày bắt đầu không được lớn hơn ngày kết thúc." });
            }

            try
            {
                var fileBytes = await _feedbackService.ExportFeedbacksToExcelAsync(startDate, endDate);
                var fileName = $"BaoCaoPhanHoi_{startDate:yyyyMMdd}_to_{endDate:yyyyMMdd}.xlsx";
                return File(
                    fileBytes, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    fileName
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi khi xuất file Excel: {ex.Message}" });
            }
        }
    }
}
