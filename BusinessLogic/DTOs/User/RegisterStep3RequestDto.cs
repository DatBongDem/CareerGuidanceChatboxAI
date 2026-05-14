using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.User
{
    public class RegisterStep3RequestDto
    {
        [Required]
        public string VerifyToken { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }
    }
}
