using BusinessLogic.Interfaces;
using DataAccess.Entities.ChatAI;
using DataAccess.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class AiEvaluationService : IAiEvaluationService
    {
        private readonly IUnitOfWork _uow;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AiEvaluationService(
            IUnitOfWork uow,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _uow = uow;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> EvaluateCategoryAsync(Guid userId, Guid categoryId)
        {
            // 1. Load category
            var category = await _uow.QuestionCategoryRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                throw new Exception("Không tìm thấy chuyên mục câu hỏi.");
            }

            // 2. Load questions in category
            var questions = (await _uow.QuestionRepository.GetAsync(q => q.CategoryId == categoryId)).ToList();
            if (!questions.Any())
            {
                throw new Exception("Chuyên mục này chưa có câu hỏi nào.");
            }

            // 3. Load user answers
            var questionIds = questions.Select(q => q.Id).ToList();
            var userAnswers = (await _uow.UserAnswerRepository.GetAsync(a => a.UserId == userId && questionIds.Contains(a.QuestionId))).ToList();

            // 4. Verify if all questions have been answered
            if (userAnswers.Count < questions.Count)
            {
                throw new Exception("Bạn cần trả lời đầy đủ tất cả câu hỏi trong chuyên mục này để nhận đánh giá từ AI.");
            }

            // 5. Build prompt
            var sb = new StringBuilder();
            sb.AppendLine($"Chuyên mục: {category.Name}");
            sb.AppendLine("Danh sách các câu hỏi và câu trả lời tương ứng của người dùng:");
            for (int i = 0; i < questions.Count; i++)
            {
                var q = questions[i];
                var ans = userAnswers.FirstOrDefault(a => a.QuestionId == q.Id);
                sb.AppendLine($"{i + 1}. Câu hỏi: {q.Content}");
                sb.AppendLine($"   Câu trả lời của người dùng: {ans?.Answer}");
            }

            var prompt = $"""
            Bạn là một chuyên gia tư vấn tuyển sinh và định hướng nghề nghiệp.
            Hãy phân tích các câu hỏi và câu trả lời của người dùng trong chuyên mục '{category.Name}' dưới đây:
            
            {sb.ToString()}
            
            Yêu cầu:
            1. Đưa ra nhận xét, đánh giá và định hướng phù hợp liên quan đến tuyển sinh, chọn ngành học, chọn trường cho người dùng này dựa trên câu trả lời của họ.
            2. **Quan trọng**: Chỉ nhận xét và đánh giá các nội dung liên quan trực tiếp đến tuyển sinh, định hướng học tập, ngành nghề. Nếu các câu trả lời của người dùng không liên quan đến tuyển sinh hoặc không cung cấp đủ thông tin định hướng học tập/tuyển sinh, hãy trả lời chính xác là: 'Không có đủ thông tin liên quan đến tuyển sinh để đánh giá phần này.'
            3. Trả lời bằng tiếng Việt, súc tích, chuyên nghiệp và có tính xây dựng. Độ dài khoảng 40-50 từ.
            4. Trả về văn bản thuần (plain text), tuyệt đối không chứa ký tự đặc biệt hay ký tự định dạng markdown như dấu sao (*), dấu thăng (#), dấu gạch ngang (-), v.v. Chỉ dùng các dấu câu cơ bản như dấu chấm, dấu phẩy.
            """;

            // 6. Call Gemini API
            var apiKey1 = _configuration["Gemini:ApiKey1"] ?? _configuration["Gemini:ApiKey"];
            var apiKey2 = _configuration["Gemini:ApiKey2"];

            if (string.IsNullOrEmpty(apiKey1) && string.IsNullOrEmpty(apiKey2))
            {
                throw new Exception("Chưa cấu hình API Key của Gemini.");
            }

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
            HttpResponseMessage response = null!;
            bool isSuccess = false;

            if (!string.IsNullOrEmpty(apiKey1))
            {
                try
                {
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    response = await _httpClient.PostAsync(
                        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey1}",
                        content);
                    if (response.IsSuccessStatusCode)
                    {
                        isSuccess = true;
                    }
                }
                catch
                {
                    if (string.IsNullOrEmpty(apiKey2)) throw;
                }
            }

            if (!isSuccess && !string.IsNullOrEmpty(apiKey2))
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                response = await _httpClient.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey2}",
                    content);
                response.EnsureSuccessStatusCode();
            }
            else if (response != null)
            {
                response.EnsureSuccessStatusCode();
            }
            else
            {
                throw new Exception("Không thể thực hiện cuộc gọi đến Gemini API.");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);

            var answer = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            var aiResponse = answer?.Trim() ?? string.Empty;

            // 7. Save or Update in database
            var existing = (await _uow.AiEvaluationRepository.GetAsync(e => e.UserId == userId && e.CategoryId == categoryId)).FirstOrDefault();
            if (existing != null)
            {
                existing.EvaluationText = aiResponse;
                existing.CreatedAt = DateTime.UtcNow;
                await _uow.AiEvaluationRepository.UpdateAsync(existing);
            }
            else
            {
                var newEval = new AiEvaluation
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CategoryId = categoryId,
                    EvaluationText = aiResponse,
                    CreatedAt = DateTime.UtcNow
                };
                await _uow.AiEvaluationRepository.AddAsync(newEval);
            }

            await _uow.SaveAsync();

            return aiResponse;
        }

        public async Task<AiEvaluation?> GetEvaluationAsync(Guid userId, Guid categoryId)
        {
            var evaluations = await _uow.AiEvaluationRepository.GetAsync(e => e.UserId == userId && e.CategoryId == categoryId);
            return evaluations.FirstOrDefault();
        }
    }
}
