using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.User
{
    public class RegisterStep2RequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Otp { get; set; }
    }
}
