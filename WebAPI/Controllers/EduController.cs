using BusinessLogic.DTOs.Edu;
using BusinessLogic.DTOs.Payment;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/edu")]
    public class EduController : ControllerBase
    {
        private readonly IEduService _eduService;

        public EduController(IEduService eduService)
        {
            _eduService = eduService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<EduRegistrationResponseDto>> RegisterEdu([FromBody] CreateEduRegistrationDto dto)
        {
            try
            {
                var result = await _eduService.RegisterEduAsync(dto);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("registrations")]
        [Authorize(Roles = "ADMIN,CONTACT,ACCOUNTANT")]
        public async Task<ActionResult<IEnumerable<EduRegistrationResponseDto>>> GetEduRegistrations()
        {
            var results = await _eduService.GetEduRegistrationsAsync();
            return Ok(results);
        }

        [HttpPost("create-payment/{registrationId}")]
        [AllowAnonymous]
        public async Task<ActionResult<CreatePaymentResponseDto>> CreateEduPayment(Guid registrationId)
        {
            try
            {
                var result = await _eduService.CreateEduPaymentLinkAsync(registrationId);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message, details = ex.ToString() });
            }
        }

        [HttpPost("import-keys/{registrationId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EduActivationKeyResponseDto>>> ImportEduKeys(Guid registrationId, IFormFile file)
        {
            try
            {
                var result = await _eduService.ImportEduKeysAsync(registrationId, file);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("activate")]
        [Authorize]
        public async Task<IActionResult> ActivateEduKey([FromBody] ActivateEduKeyRequestDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                await _eduService.ActivateEduKeyAsync(userId, dto.ActivationKey);
                return Ok(new { message = "Kích hoạt tài khoản thành công." });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("by-transaction/{transactionCode}")]
        [AllowAnonymous]
        public async Task<ActionResult<EduRegistrationResponseDto>> GetByTransactionCode(string transactionCode)
        {
            var result = await _eduService.GetEduRegistrationByTransactionCodeAsync(transactionCode);
            if (result == null)
            {
                return NotFound(new { message = "Không tìm thấy đăng ký với mã giao dịch này." });
            }
            return Ok(result);
        }

        [HttpPut("update-status/{registrationId}")]
        [AllowAnonymous]
        public async Task<ActionResult<EduRegistrationResponseDto>> UpdateStatus(Guid registrationId, [FromQuery] string status)
        {
            try
            {
                var result = await _eduService.UpdateEduRegistrationStatusAsync(registrationId, status);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message, details = ex.ToString() });
            }
        }
    }
}
