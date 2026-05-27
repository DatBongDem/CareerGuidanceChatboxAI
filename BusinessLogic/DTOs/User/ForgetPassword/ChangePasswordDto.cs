using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.User.ForgetPassword
{
    public class ChangePasswordDto
    {
        public string OldPassword { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;
    }
}
