using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.User.ForgetPassword
{
    public class ResetPasswordDto
    {
        public string Email { get; set; } = string.Empty;

        public string Otp { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;
    }
}
