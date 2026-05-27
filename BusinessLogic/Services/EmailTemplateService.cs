using BusinessLogic.EmailTemplates;
using BusinessLogic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        public string GetForgotPasswordOtpTemplate(string email, string otp)
        {
            return ForgotPasswordEmailTemplate.Template(email, otp);
        }

        public string GetRegisterOtpTemplate(string email, string otp)
        {
            return RegisterEmailTemplate.TemplateRegister(email, otp);
        }
    }
}
