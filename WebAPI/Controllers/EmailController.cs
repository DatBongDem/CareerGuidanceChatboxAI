using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send-test-email")]
        public async Task<IActionResult> SendTestEmail([FromQuery] string toEmail, [FromQuery] string subject, [FromQuery] string message)
        {
            if (string.IsNullOrEmpty(toEmail) || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(message))
            {
                return BadRequest("To email, subject, and message cannot be empty.");
            }

            try
            {
                await _emailService.SendEmailAsync(toEmail, subject, message);
                return Ok("Test email sent successfully!");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error sending test email: {ex.Message}");
            }
        }
    }
}
