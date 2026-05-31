using BusinessLogic.Interfaces;
using DataAccess.Entities.ChatAI;
using DataAccess.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ChatService(
            IUnitOfWork unitOfWork,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> AskAIAsync(Guid userId, string question)
        {
            var apiKey = _configuration["Gemini:ApiKey"];

            var prompt = $"""
        Bạn là chuyên gia tư vấn hướng nghiệp.

        Trả lời bằng tiếng Việt.
        Đưa ra lời khuyên rõ ràng.

        Câu hỏi:
        {question}
        """;

            var requestBody = new
            {
                contents = new[]
                {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
            };

            var json = JsonSerializer.Serialize(requestBody);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}",
                content);

            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(responseJson);

            var answer = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            var history = new ChatHistory
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Question = question,
                Answer = answer ?? "",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ChatHistoryRepository.AddAsync(history);
            await _unitOfWork.SaveAsync();

            return answer ?? "";
        }
    }
}
