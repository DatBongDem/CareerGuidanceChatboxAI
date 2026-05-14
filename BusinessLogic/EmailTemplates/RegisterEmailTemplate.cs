using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.EmailTemplates
{
    public static class RegisterEmailTemplate
    {
        public static string TemplateRegister(string email, string otp)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
<meta charset=""UTF-8"">
<title>Welcome to 4S System</title>
</head>

<body style=""margin:0; padding:0; background:#f7f7f7; font-family:Arial,Helvetica,sans-serif;"">
    
<table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f7f7f7; padding:30px 0;"">
<tr>
<td align=""center"">

<table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background:#ffffff; border-radius:12px; padding:40px; box-shadow:0 6px 25px rgba(0,0,0,0.08);"">
    
    <!-- HEADER -->
    <tr>
        <td align=""center"" style=""padding-bottom:20px;"">
            <h1 style=""margin:0; color:#D4AF37; letter-spacing:2px;"">4S SYSTEM</h1>
            <div style=""width:60px;height:3px;background:#D4AF37;margin:15px auto 0;""></div>
        </td>
    </tr>

    <!-- TITLE -->
    <tr>
        <td>
            <h2 style=""color:#333; margin-bottom:10px;"">Welcome to 4S 🎉</h2>
        </td>
    </tr>

    <!-- CONTENT -->
    <tr>
        <td style=""color:#555; font-size:15px; line-height:1.6;"">
            Hello <b>{email}</b>,<br><br>
            Thank you for creating your <b>4S System</b> account.  
            To get started, please confirm your email using the verification code below.
        </td>
    </tr>

    <!-- OTP BOX -->
    <tr>
        <td align=""center"" style=""padding:35px 0;"">
            <div style=""
                display:inline-block;
                font-size:36px;
                font-weight:bold;
                letter-spacing:10px;
                padding:18px 35px;
                background:#fffaf0;
                border-radius:10px;
                color:#D4AF37;
                border:2px solid #D4AF37;"">
                {otp}
            </div>
        </td>
    </tr>

    <!-- EXPIRY -->
    <tr>
        <td style=""color:#555; font-size:14px;"">
            This verification code will expire in <b style=""color:#D4AF37;"">10 minutes</b>.<br>
            Please do not share this code with anyone.
        </td>
    </tr>

    <!-- HELP -->
    <tr>
        <td style=""padding-top:25px; color:#777; font-size:14px;"">
            If you did not sign up for 4S System, you can safely ignore this email.
        </td>
    </tr>

    <!-- FOOTER -->
    <tr>
        <td style=""padding-top:40px; border-top:1px solid #eee; font-size:12px; color:#999;"">
            © 2026 4S System. All rights reserved.<br>
            This is an automated email — please do not reply.
        </td>
    </tr>

</table>

</td>
</tr>
</table>

</body>
</html>";
        }
    }
}
