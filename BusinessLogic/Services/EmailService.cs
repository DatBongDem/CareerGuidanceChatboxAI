using BusinessLogic.DTOs.Email;
using BusinessLogic.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Net;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("4sCompany", _emailSettings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = message;
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.Timeout = 10000; // 10 seconds timeout to prevent hanging on Render's 30s limit
            smtp.ServerCertificateValidationCallback = (s, c, h, e) => true; // Bypass certificate validation when connecting via resolved IP

            string smtpHost = _emailSettings.SmtpServer;
            try
            {
                // Resolve hostname to IPv4 to bypass Render's IPv6 resolution routing bugs
                var addresses = await Dns.GetHostAddressesAsync(smtpHost);
                foreach (var address in addresses)
                {
                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        smtpHost = address.ToString();
                        break;
                    }
                }
            }
            catch
            {
                // Fallback to configuration value if DNS resolution fails
            }

            await smtp.ConnectAsync(smtpHost, _emailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.AppPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
