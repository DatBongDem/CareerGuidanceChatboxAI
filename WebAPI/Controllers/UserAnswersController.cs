using BusinessLogic.Interfaces;
using DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BusinessLogic.DTOs.ChatAI.UserAnswer;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserAnswersController : ControllerBase
    {
        private readonly IUserAnswerService _service;

        public UserAnswersController(IUserAnswerService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { success = false, message = "User is not logged in." });
            }
            var userId = Guid.Parse(userIdClaim);

            var data = await _service.GetByUserIdAsync(userId);

            return Ok(new
            {
                success = true,
                message = "Get all user answers",
                data
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserAnswerDto dto)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { success = false, message = "User is not logged in." });
            }
            var userId = Guid.Parse(userIdClaim);

            var model = new UserAnswer
            {
                UserId = userId,
                QuestionId = dto.QuestionId,
                Answer = dto.Answer
            };

            try
            {
                var result = await _service.CreateAsync(model);
                return StatusCode(201, new
                {
                    success = true,
                    message = "Created",
                    data = result
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

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { success = false, message = "User is not logged in." });
            }
            var userId = Guid.Parse(userIdClaim);

            var success = await _service.DeleteByUserIdAsync(userId);

            if (!success)
                return NotFound(new
                {
                    success = false,
                    message = "No answers found to delete"
                });

            return Ok(new
            {
                success = true,
                message = "Deleted all user answers successfully"
            });
        }
    }
}