using BusinessLogic.DTOs.Statistics;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/web-stats")]
    [ApiController]
    public class WebStatsController : ControllerBase
    {
        private readonly IWebStatsService _webStatsService;

        public WebStatsController(IWebStatsService webStatsService)
        {
            _webStatsService = webStatsService;
        }

        [HttpGet("visits")]
        public async Task<IActionResult> GetDailyWebVisits()
        {
            var data = await _webStatsService.GetDailyWebVisitsAsync();
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách lượt truy cập web hàng ngày thành công.",
                data
            });
        }

        [HttpPost("visits/increment")]
        public async Task<IActionResult> IncrementDailyWebVisits()
        {
            var newCount = await _webStatsService.IncrementDailyWebVisitsAsync();
            return Ok(new
            {
                success = true,
                message = "Đã tăng số lượt truy cập web thành công.",
                currentCount = newCount
            });
        }

        [HttpGet("user-visits")]
        public async Task<IActionResult> GetDailyUserVisits()
        {
            var data = await _webStatsService.GetDailyUserVisitsAsync();
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách tài khoản truy cập hàng ngày thành công.",
                data
            });
        }

        [HttpPost("user-visits/record")]
        public async Task<IActionResult> RecordDailyUserVisit([FromBody] RecordUserVisitDto dto)
        {
            Guid? targetUserId = dto.UserId;

            // If not provided in body, attempt to read from JWT claims
            if (!targetUserId.HasValue)
            {
                var userIdClaim = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out var parsedUserId))
                {
                    targetUserId = parsedUserId;
                }
            }

            if (!targetUserId.HasValue)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "UserId không được cung cấp trong body và không tìm thấy trong token xác thực."
                });
            }

            try
            {
                await _webStatsService.RecordDailyUserVisitAsync(targetUserId.Value);
                return Ok(new
                {
                    success = true,
                    message = "Đã ghi nhận tài khoản truy cập hôm nay thành công."
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Có lỗi xảy ra: {ex.Message}"
                });
            }
        }
    }
}
