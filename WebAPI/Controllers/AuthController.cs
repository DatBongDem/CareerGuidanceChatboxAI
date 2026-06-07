using BusinessLogic.DTOs.User;
using BusinessLogic.DTOs.User.ForgetPassword;
using BusinessLogic.Interfaces;
using DataAccess.Interfaces;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IAvatarService _avatarService;
        public AuthController(IAuthService authService, IAvatarService avatarService)
        {
            _authService = authService;
            _avatarService = avatarService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            try
            {
                var result = await _authService.Login(loginDto);

                Response.Cookies.Append(
                    "refreshToken",
                    result.RefreshToken,
                    new CookieOptions
                    {
                        HttpOnly = true,

                        Secure = true,

                        SameSite = SameSiteMode.None,

                        Expires = DateTime.UtcNow.AddDays(7)
                    });

                return Ok(new
                {
                    accessToken = result.AccessToken
                });
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

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest("Refresh token not found.");
            }

            await _authService.Logout(
                new LogoutDto
                {
                    RefreshToken = refreshToken
                });

            Response.Cookies.Delete("refreshToken");

            return Ok(new
            {
                message = "Logout successful."
            });
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken =
                Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized();
            }

            var result = await _authService
                .RefreshToken(refreshToken);

            Response.Cookies.Append(
                "refreshToken",
                result.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,

                    Secure = true,

                    SameSite = SameSiteMode.Strict,

                    Expires = DateTime.UtcNow.AddDays(7)
                });

            return Ok(new
            {
                accessToken = result.AccessToken
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userIdClaim = User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim);

            var result = await _authService.GetMe(userId);

            return Ok(result);
        }
        [Authorize]
        [HttpPost("upload-avatar")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("User ID not found in token.");
            }

            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return BadRequest("Invalid user ID format.");
            }

            var url = await _avatarService.UploadAvatarAsync(file, userId);

            return Ok(new
            {
                avatarUrl = url
            });
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("User ID not found in token.");
            }

            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return BadRequest("Invalid user ID format.");
            }

            try
            {
                var result = await _authService.UpdateProfileAsync(userId, updateProfileDto);
                return Ok(result);
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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordDto dto)
        {
            try
            {
                await _authService.ForgotPassword(dto);

                return Ok(new
                {
                    message = "OTP sent successfully."
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordDto dto)
        {
            try
            {
                await _authService.ResetPassword(dto);

                return Ok(new
                {
                    message = "Password reset successfully."
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            try
            {
                var email = User.FindFirst(
                    ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    return Unauthorized(new
                    {
                        message = "Invalid token."
                    });
                }

                await _authService.ChangePassword(
                    email,
                    dto);

                return Ok(new
                {
                    message = "Password changed successfully."
                });
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}