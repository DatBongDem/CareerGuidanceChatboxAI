using BusinessLogic.DTOs.User;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var token = await _authService.Login(loginDto);
                return Ok(new { Token = token });
            }
            catch (ApplicationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpPost("logout")]
        [Authorize] // Requires a valid token to access
        public IActionResult Logout()
        {
            // For stateless JWT, logout is typically handled client-side by deleting the token.
            // This endpoint can be used to confirm token validity and provide a server-side acknowledgement.
            return Ok(new { message = "Logged out successfully." });
        }
    }
}
