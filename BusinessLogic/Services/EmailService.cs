using BusinessLogic.DTOs.Email;
using BusinessLogic.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http;
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
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("api-key", _emailSettings.AppPassword);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var payload = new
            {
                sender = new
                {
                    name = "4sCompany",
                    email = _emailSettings.SenderEmail
                },
                to = new[]
                {
                    new
                    {
                        email = toEmail
                    }
                },
                subject = subject,
                htmlContent = message
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", content);
            if (!response.IsSuccessStatusCode)
            {
                var errorDetail = await response.Content.ReadAsStringAsync();
                throw new System.Exception($"Failed to send email via Brevo API. Status: {response.StatusCode}, Details: {errorDetail}");
            }
        }
    }
}
