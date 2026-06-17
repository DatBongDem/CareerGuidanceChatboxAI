using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserAiSummariesController : ControllerBase
    {
        private readonly IUserAiSummaryService _userAiSummaryService;

        public UserAiSummariesController(IUserAiSummaryService userAiSummaryService)
        {
            _userAiSummaryService = userAiSummaryService;
        }

        [HttpPost("evaluate")]
        public async Task<IActionResult> EvaluateOverall()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { success = false, message = "User is not logged in." });
            }

            try
            {
                var userId = Guid.Parse(userIdClaim);
                var summary = await _userAiSummaryService.EvaluateOverallAsync(userId);
                return Ok(new
                {
                    success = true,
                    message = "Overall summary evaluation generated successfully.",
                    data = summary
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOverallSummary()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { success = false, message = "User is not logged in." });
            }

            try
            {
                var userId = Guid.Parse(userIdClaim);
                var summary = await _userAiSummaryService.GetOverallSummaryAsync(userId);
                if (summary == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Chưa có đánh giá tổng kết nào cho người dùng này."
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = summary
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
