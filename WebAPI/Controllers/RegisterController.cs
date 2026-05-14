using BusinessLogic.DTOs.User;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly IAuthService _authService;

        public RegisterController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register-step1")]
        public async Task<IActionResult> RegisterStep1([FromBody] RegisterStep1RequestDto request)
        {
            try
            {
                await _authService.RegisterStep1(request);
                return Ok("OTP sent to your email. Please proceed to step 2.");
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] RegisterStep2RequestDto request)
        {
            try
            {
                string verifyToken = await _authService.VerifyOtp(request);
                return Ok(new { verifyToken = verifyToken });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("register-step3")]
        public async Task<IActionResult> RegisterStep3([FromBody] RegisterStep3RequestDto request)
        {
            try
            {
                await _authService.RegisterStep3(request);
                return Ok("Registration successful!");
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
