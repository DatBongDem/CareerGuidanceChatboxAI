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
    public class AiEvaluationsController : ControllerBase
    {
        private readonly IAiEvaluationService _aiEvaluationService;

        public AiEvaluationsController(IAiEvaluationService aiEvaluationService)
        {
            _aiEvaluationService = aiEvaluationService;
        }

        [HttpPost("evaluate/{categoryId}")]
        public async Task<IActionResult> EvaluateCategory(Guid categoryId)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { success = false, message = "User is not logged in." });
            }

            try
            {
                var userId = Guid.Parse(userIdClaim);
                var evaluation = await _aiEvaluationService.EvaluateCategoryAsync(userId, categoryId);
                return Ok(new
                {
                    success = true,
                    message = "Evaluation generated successfully.",
                    data = evaluation
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

        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetEvaluation(Guid categoryId)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { success = false, message = "User is not logged in." });
            }

            try
            {
                var userId = Guid.Parse(userIdClaim);
                var evaluation = await _aiEvaluationService.GetEvaluationAsync(userId, categoryId);
                if (evaluation == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Chưa có đánh giá nào cho chuyên mục này."
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = evaluation
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
