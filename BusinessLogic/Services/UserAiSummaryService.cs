using BusinessLogic.DTOs.ChatAI;
using BusinessLogic.Interfaces;
using DataAccess.Entities.ChatAI;
using DataAccess.Interfaces;
using DataAccess.Shares;
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
    public class UserAiSummaryService : IUserAiSummaryService
    {
        private readonly IUnitOfWork _uow;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public UserAiSummaryService(
            IUnitOfWork uow,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _uow = uow;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<UserAiSummaryResponseDto> EvaluateOverallAsync(Guid userId)
        {
            // 1. Fetch active questions
            var activeQuestions = (await _uow.QuestionRepository.GetAsync(q => q.IsActice == StatusEnum.Yes)).ToList();
            if (!activeQuestions.Any())
            {
                throw new Exception("Hệ thống chưa cấu hình câu hỏi nào.");
            }

            // 2. Fetch user answers
            var questionIds = activeQuestions.Select(q => q.Id).ToList();
            var userAnswers = (await _uow.UserAnswerRepository.GetAsync(a => a.UserId == userId && questionIds.Contains(a.QuestionId))).ToList();

            // 3. Verify that all questions are answered
            if (userAnswers.Count < activeQuestions.Count)
            {
                throw new Exception("Bạn cần hoàn thành trả lời đầy đủ tất cả câu hỏi của toàn bộ các chuyên mục để nhận nhận xét tổng quan và đề xuất trường đại học.");
            }

            // 4. Load all categories and construct prompt details
            var categories = (await _uow.QuestionCategoryRepository.GetAsync()).OrderBy(c => c.DisplayOrder).ToList();
            var sb = new StringBuilder();

            foreach (var cat in categories)
            {
                var catQuestions = activeQuestions.Where(q => q.CategoryId == cat.Id).OrderBy(q => q.DisplayOrder).ToList();
                if (!catQuestions.Any()) continue;

                sb.AppendLine($"Chuyên mục: {cat.Name}");
                for (int i = 0; i < catQuestions.Count; i++)
                {
                    var q = catQuestions[i];
                    var ans = userAnswers.FirstOrDefault(a => a.QuestionId == q.Id);
                    sb.AppendLine($"  {i + 1}. Câu hỏi: {q.Content}");
                    sb.AppendLine($"     Câu trả lời: {ans?.Answer}");
                }
            }

            // 5. Load all universities
            var universities = (await _uow.UniversityRepository.GetAsync()).ToList();
            var uniListSb = new StringBuilder();
            foreach (var uni in universities)
            {
                uniListSb.AppendLine($"- [ID: {uni.UniversityId}] Tên: {uni.Name} ({uni.ShortName}), Địa chỉ: {uni.Location}, Xếp hạng: {uni.Ranking}");
            }

            // 6. Build prompt
            var prompt = $@"
Bạn là một chuyên gia tư vấn tuyển sinh và định hướng nghề nghiệp hàng đầu.
Hãy phân tích toàn bộ câu trả lời của người dùng dưới đây ở tất cả các chuyên mục để đưa ra đánh giá tổng quan và đề xuất trường đại học phù hợp nhất cho họ.

Dữ liệu câu trả lời của người dùng:
{sb.ToString()}

Danh sách các trường đại học hiện có trong hệ thống của chúng tôi:
{uniListSb.ToString()}

Yêu cầu:
1. Đưa ra nhận xét, đánh giá tổng quan (nhận xét chung) về thế mạnh, sở thích nghề nghiệp, định hướng tuyển sinh của người dùng dựa trên tất cả câu trả lời của họ.
2. Từ danh sách các trường đại học được cung cấp ở trên, hãy chọn ra:
   - 3 trường đại học phù hợp nhất (danh sách 'top3' chứa các ID dạng Guid tương ứng).
   - 5 trường đại học phù hợp nhì (danh sách 'next5' chứa các ID dạng Guid tương ứng).
   *Chú ý: Tuyệt đối chỉ chọn từ danh sách ID được cung cấp ở trên. Không tự ý bịa ra ID trường nằm ngoài danh sách. Nếu số lượng trường trong hệ thống ít hơn, hãy xếp tất cả các trường có thể vào 'top3' hoặc 'next5' và để trống phần còn lại.*
3. Trả về kết quả dưới dạng JSON hợp lệ khớp chính xác với cấu trúc C# sau:
{{
  ""summaryText"": ""Nội dung nhận xét tổng quan bằng tiếng Việt... (dùng markdown nếu cần thiết)"",
  ""top3"": [""guid1"", ""guid2"", ""guid3""],
  ""next5"": [""guid1"", ""guid2"", ""guid3"", ""guid4"", ""guid5""]
}}
Vui lòng không trả về bất kỳ văn bản nào khác ngoài khối JSON này.
";

            // 7. Request Gemini API in JSON mode
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
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
                },
                generationConfig = new
                {
                    responseMimeType = "application/json"
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}",
                content);

            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);

            var rawAnswerJson = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrEmpty(rawAnswerJson))
            {
                throw new Exception("Không nhận được câu trả lời từ AI.");
            }

            // 8. Parse JSON response
            var geminiResult = JsonSerializer.Deserialize<GeminiSummaryResponse>(rawAnswerJson);
            if (geminiResult == null)
            {
                throw new Exception("Lỗi phân tích kết quả phản hồi của AI.");
            }

            var top3Guids = geminiResult.top3
                .Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .ToList();

            var next5Guids = geminiResult.next5
                .Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .ToList();

            var top3Str = string.Join(",", top3Guids);
            var next5Str = string.Join(",", next5Guids);

            // 9. Save or Update in database
            var existing = (await _uow.UserAiSummaryRepository.GetAsync(s => s.UserId == userId)).FirstOrDefault();
            Guid entityId;
            if (existing != null)
            {
                entityId = existing.Id;
                existing.SummaryText = geminiResult.summaryText;
                existing.Top3UniversityIds = top3Str;
                existing.Next5UniversityIds = next5Str;
                existing.CreatedAt = DateTime.UtcNow;
                await _uow.UserAiSummaryRepository.UpdateAsync(existing);
            }
            else
            {
                entityId = Guid.NewGuid();
                var newSummary = new UserAiSummary
                {
                    Id = entityId,
                    UserId = userId,
                    SummaryText = geminiResult.summaryText,
                    Top3UniversityIds = top3Str,
                    Next5UniversityIds = next5Str,
                    CreatedAt = DateTime.UtcNow
                };
                await _uow.UserAiSummaryRepository.AddAsync(newSummary);
            }

            await _uow.SaveAsync();

            // 10. Load detailed university data
            var allTargetIds = top3Guids.Concat(next5Guids).Distinct().ToList();
            var loadedUnis = (await _uow.UniversityRepository.GetAsync(u => allTargetIds.Contains(u.UniversityId))).ToList();

            var top3Unis = top3Guids
                .Select(id => loadedUnis.FirstOrDefault(u => u.UniversityId == id))
                .Where(u => u != null)
                .Select(u => new UniversityDto
                {
                    UniversityId = u!.UniversityId,
                    Name = u.Name,
                    ShortName = u.ShortName,
                    Location = u.Location,
                    Ranking = u.Ranking,
                    Avatar = u.Avatar
                })
                .ToList();

            var next5Unis = next5Guids
                .Select(id => loadedUnis.FirstOrDefault(u => u.UniversityId == id))
                .Where(u => u != null)
                .Select(u => new UniversityDto
                {
                    UniversityId = u!.UniversityId,
                    Name = u.Name,
                    ShortName = u.ShortName,
                    Location = u.Location,
                    Ranking = u.Ranking,
                    Avatar = u.Avatar
                })
                .ToList();

            return new UserAiSummaryResponseDto
            {
                Id = entityId,
                UserId = userId,
                SummaryText = geminiResult.summaryText,
                CreatedAt = DateTime.UtcNow,
                Top3Universities = top3Unis,
                Next5Universities = next5Unis
            };
        }

        public async Task<UserAiSummaryResponseDto?> GetOverallSummaryAsync(Guid userId)
        {
            var summary = (await _uow.UserAiSummaryRepository.GetAsync(s => s.UserId == userId)).FirstOrDefault();
            if (summary == null) return null;

            var top3Guids = summary.Top3UniversityIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .ToList();

            var next5Guids = summary.Next5UniversityIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
                .Where(g => g != Guid.Empty)
                .ToList();

            var allTargetIds = top3Guids.Concat(next5Guids).Distinct().ToList();
            var loadedUnis = (await _uow.UniversityRepository.GetAsync(u => allTargetIds.Contains(u.UniversityId))).ToList();

            var top3Unis = top3Guids
                .Select(id => loadedUnis.FirstOrDefault(u => u.UniversityId == id))
                .Where(u => u != null)
                .Select(u => new UniversityDto
                {
                    UniversityId = u!.UniversityId,
                    Name = u.Name,
                    ShortName = u.ShortName,
                    Location = u.Location,
                    Ranking = u.Ranking,
                    Avatar = u.Avatar
                })
                .ToList();

            var next5Unis = next5Guids
                .Select(id => loadedUnis.FirstOrDefault(u => u.UniversityId == id))
                .Where(u => u != null)
                .Select(u => new UniversityDto
                {
                    UniversityId = u!.UniversityId,
                    Name = u.Name,
                    ShortName = u.ShortName,
                    Location = u.Location,
                    Ranking = u.Ranking,
                    Avatar = u.Avatar
                })
                .ToList();

            return new UserAiSummaryResponseDto
            {
                Id = summary.Id,
                UserId = summary.UserId,
                SummaryText = summary.SummaryText,
                CreatedAt = summary.CreatedAt,
                Top3Universities = top3Unis,
                Next5Universities = next5Unis
            };
        }

        private class GeminiSummaryResponse
        {
            public string summaryText { get; set; } = string.Empty;
            public List<string> top3 { get; set; } = new List<string>();
            public List<string> next5 { get; set; } = new List<string>();
        }
    }
}
