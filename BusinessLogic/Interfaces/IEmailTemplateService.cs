using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IEmailTemplateService
    {
        string GetRegisterOtpTemplate(string email, string otp);
        string GetForgotPasswordOtpTemplate(string email, string otp);

    }
}
