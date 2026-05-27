using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.EmailTemplates
{
    public static class ForgotPasswordEmailTemplate
    {
        public static string Template(
            string email,
            string otp)
        {
            return $@"
            <div style='
                font-family: Arial, sans-serif;
                background-color: #f4f4f4;
                padding: 40px;
            '>

                <div style='
                    max-width: 600px;
                    margin: auto;
                    background: white;
                    border-radius: 10px;
                    padding: 40px;
                    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                '>

                    <h2 style='
                        color: #dc2626;
                        text-align: center;
                    '>
                        Reset Your Password
                    </h2>

                    <p style='font-size: 16px; color: #333;'>
                        Hello <b>{email}</b>,
                    </p>

                    <p style='font-size: 16px; color: #333;'>
                        We received a request to reset your password.
                    </p>

                    <p style='font-size: 16px; color: #333;'>
                        Please use the OTP below to continue:
                    </p>

                    <div style='
                        text-align: center;
                        margin: 30px 0;
                    '>
                        <span style='
                            display: inline-block;
                            background-color: #dc2626;
                            color: white;
                            font-size: 32px;
                            font-weight: bold;
                            letter-spacing: 8px;
                            padding: 15px 30px;
                            border-radius: 10px;
                        '>
                            {otp}
                        </span>
                    </div>

                    <p style='
                        font-size: 14px;
                        color: #666;
                    '>
                        This OTP will expire in
                        <b>10 minutes</b>.
                    </p>

                    <p style='
                        font-size: 14px;
                        color: #666;
                    '>
                        If you did not request a password reset,
                        please ignore this email.
                    </p>

                    <hr style='margin: 30px 0;' />

                    <p style='
                        font-size: 12px;
                        color: #999;
                        text-align: center;
                    '>
                        © 2026 Your System.
                        All rights reserved.
                    </p>

                </div>
            </div>";
        }
    }
}
