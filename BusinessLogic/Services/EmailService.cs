using BusinessLogic.DTOs.Email;
using BusinessLogic.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly IHttpClientFactory _httpClientFactory;

        public EmailService(IOptions<EmailSettings> emailSettings, IHttpClientFactory httpClientFactory)
        {
            _emailSettings = emailSettings.Value;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            /*
            // --- COMMENTED OUT OLD SMTP GMAIL CODE ---
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_emailSettings.SenderEmail);
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
            await smtp.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.AppPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
            // ----------------------------------------
            */

            // --- USE RESEND HTTP API ---
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _emailSettings.AppPassword);

            var payload = new
            {
                from = _emailSettings.SenderEmail, // onboarding@resend.dev
                to = toEmail,
                subject = subject,
                html = message
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.resend.com/emails", content);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new ApplicationException($"Failed to send email via Resend API. Status: {response.StatusCode}, Details: {errorResponse}");
            }
        }
    }
}
