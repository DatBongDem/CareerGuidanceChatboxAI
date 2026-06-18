using BusinessLogic.DTOs.ChatAI;
using BusinessLogic.Interfaces;
using DataAccess.Entities;
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
            // 1. Fetch non-chat-AI categories
            var categories = (await _uow.QuestionCategoryRepository.GetAsync(c => !c.IsChatAi)).OrderBy(c => c.DisplayOrder).ToList();
            var categoryIds = categories.Select(c => c.Id).ToList();

            // 2. Fetch active questions in these traditional categories
            var activeQuestions = (await _uow.QuestionRepository.GetAsync(q => q.IsActice == StatusEnum.Yes && categoryIds.Contains(q.CategoryId))).ToList();
            if (!activeQuestions.Any())
            {
                throw new Exception("Hệ thống chưa cấu hình câu hỏi nào cho chuyên mục định hướng.");
            }

            // 3. Fetch user answers
            var questionIds = activeQuestions.Select(q => q.Id).ToList();
            var userAnswers = (await _uow.UserAnswerRepository.GetAsync(a => a.UserId == userId && questionIds.Contains(a.QuestionId))).ToList();

            // 4. Verify that all questions are answered
            if (userAnswers.Count < activeQuestions.Count)
            {
                throw new Exception("Bạn cần hoàn thành trả lời đầy đủ tất cả câu hỏi của toàn bộ các chuyên mục định hướng để nhận nhận xét tổng quan và đề xuất trường đại học.");
            }

            // 4.5. Check cache: if summary already exists and answers haven't changed since then
            var cachedSumm = (await _uow.UserAiSummaryRepository.GetAsync(s => s.UserId == userId)).FirstOrDefault();
            if (cachedSumm != null && userAnswers.Any())
            {
                var maxAnsweredAt = userAnswers.Max(a => a.AnsweredAt);
                if (cachedSumm.CreatedAt >= maxAnsweredAt)
                {
                    var cachedSummary = await GetOverallSummaryAsync(userId);
                    if (cachedSummary != null)
                    {
                        return cachedSummary;
                    }
                }
            }

            // 5. Construct prompt details
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

            // 5. Load all universities, majors and university-majors
            var universities = (await _uow.UniversityRepository.GetAsync()).ToList();
            var universityMajors = (await _uow.UniversityMajorRepository.GetAsync()).ToList();
            var majors = (await _uow.MajorRepository.GetAsync()).ToList();

            var uniListSb = new StringBuilder();
            foreach (var uni in universities)
            {
                var uniMajorIds = universityMajors.Where(um => um.UniversityId == uni.UniversityId).Select(um => um.MajorId).ToList();
                var uniMajors = majors.Where(m => uniMajorIds.Contains(m.MajorId)).ToList();
                var majorStrings = uniMajors.Select(m => $"[ID Ngành: {m.MajorId}] {m.Name ?? "Chưa đặt tên"}").ToList();
                var majorListStr = majorStrings.Any() ? string.Join(", ", majorStrings) : "Không có ngành học nào được đăng ký";

                uniListSb.AppendLine($"- [ID Trường: {uni.UniversityId}] Tên: {uni.Name} ({uni.ShortName}), Địa chỉ: {uni.Location}, Xếp hạng: {uni.Ranking}");
                uniListSb.AppendLine($"  Các ngành học đào tạo: {majorListStr}");
            }

            // 6. Build prompt
            var prompt = $@"
Bạn là một chuyên gia tư vấn tuyển sinh và định hướng nghề nghiệp hàng đầu.
Hãy phân tích toàn bộ câu trả lời của người dùng dưới đây ở tất cả các chuyên mục để đưa ra đánh giá tổng quan và đề xuất trường đại học cùng ngành học phù hợp nhất cho họ.

Dữ liệu câu trả lời của người dùng:
{sb.ToString()}

Danh sách các trường đại học hiện có và các ngành học đào tạo tương ứng trong hệ thống của chúng tôi:
{uniListSb.ToString()}

Yêu cầu:
1. Đưa ra nhận xét, đánh giá tổng quan (nhận xét chung) về thế mạnh, sở thích nghề nghiệp, định hướng tuyển sinh của người dùng dựa trên tất cả câu trả lời của họ.
2. Từ danh sách các trường đại học được cung cấp ở trên, hãy chọn ra:
   - 3 trường đại học phù hợp nhất (danh sách 'top3' chứa các đề xuất tương ứng).
   - 5 trường đại học phù hợp nhì (danh sách 'next5' chứa các đề xuất tương ứng).
   *Chú ý: Đối với mỗi trường đại học được chọn, hãy gợi ý từ 1-2 ngành học đào tạo phù hợp nhất của chính trường đó (sử dụng đúng ID Ngành được cung cấp dưới trường đó) và đánh giá tỷ lệ phần trăm độ phù hợp của trường này với người dùng (là số nguyên từ 0 đến 100, ví dụ 80, 75, lưu vào trường 'matchPercentage').*
   *Tuyệt đối chỉ chọn từ danh sách ID Trường và ID Ngành học được cung cấp ở trên. Không tự ý bịa ra ID nằm ngoài danh sách.*
3. Trả về kết quả dưới dạng JSON hợp lệ khớp chính xác với cấu trúc sau:
{{
  ""summaryText"": ""Nội dung nhận xét tổng quan bằng tiếng Việt... (dùng markdown nếu cần thiết)"",
  ""top3"": [
    {{
      ""universityId"": ""id-truong-1"",
      ""matchPercentage"": 85,
      ""majorIds"": [""id-nganh-1"", ""id-nganh-2""]
    }}
  ],
  ""next5"": [
    {{
      ""universityId"": ""id-truong-2"",
      ""matchPercentage"": 70,
      ""majorIds"": [""id-nganh-3""]
    }}
  ]
}}
Vui lòng không trả về bất kỳ văn bản nào khác ngoài khối JSON này.
";

            // 7. Request Gemini API in JSON mode
            var apiKeys = new List<string>();
            var k1 = _configuration["Gemini:ApiKey1"] ?? _configuration["Gemini:ApiKey"];
            var k2 = _configuration["Gemini:ApiKey2"];
            var k3 = _configuration["Gemini:ApiKey3"];

            if (!string.IsNullOrEmpty(k1)) apiKeys.Add(k1);
            if (!string.IsNullOrEmpty(k2)) apiKeys.Add(k2);
            if (!string.IsNullOrEmpty(k3)) apiKeys.Add(k3);

            if (!apiKeys.Any())
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
            HttpResponseMessage response = null!;
            bool isSuccess = false;
            string lastError = "";

            var models = new List<string>
            {
                "gemini-2.5-flash",
                "gemini-3.5-flash",
                "gemini-2.5-flash-lite",
                "gemini-3-flash-preview",
                "gemini-2.0-flash",
                "gemini-2.0-flash-lite",
                "gemini-2.5-pro",
                "gemini-3.1-pro-preview",
                "gemini-3.1-flash-lite"
            };

            foreach (var model in models)
            {
                foreach (var key in apiKeys)
                {
                    try
                    {
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        response = await _httpClient.PostAsync(
                            $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={key}",
                            content);
                        if (response.IsSuccessStatusCode)
                        {
                            isSuccess = true;
                            break;
                        }
                        else
                        {
                            var errBody = await response.Content.ReadAsStringAsync();
                            lastError = $"Model: {model}, Status: {response.StatusCode}, Body: {errBody}";
                        }
                    }
                    catch (Exception ex)
                    {
                        lastError = $"Model: {model}, Error: {ex.Message}";
                    }
                }

                if (isSuccess)
                {
                    break;
                }
            }

            if (!isSuccess)
            {
                throw new Exception($"Không thể thực hiện cuộc gọi đến Gemini API với các API Key và Model hiện có. Lỗi gần nhất: {lastError}");
            }

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

            var top3Str = JsonSerializer.Serialize(geminiResult.top3);
            var next5Str = JsonSerializer.Serialize(geminiResult.next5);

            // 9. Save or Update in database
            var existing = (await _uow.UserAiSummaryRepository.GetAsync(s => s.UserId == userId)).FirstOrDefault();
            Guid entityId;
            if (existing != null)
            {
                entityId = existing.Id;
                existing.SummaryText = geminiResult.summaryText;
                existing.Top3Recommendations = top3Str;
                existing.Next5Recommendations = next5Str;
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
                    Top3Recommendations = top3Str,
                    Next5Recommendations = next5Str,
                    CreatedAt = DateTime.UtcNow
                };
                await _uow.UserAiSummaryRepository.AddAsync(newSummary);
            }

            await _uow.SaveAsync();

            // 10. Map detailed data
            return MapToResponseDto(entityId, userId, geminiResult.summaryText, DateTime.UtcNow, geminiResult.top3, geminiResult.next5, universities, majors);
        }

        public async Task<UserAiSummaryResponseDto?> GetOverallSummaryAsync(Guid userId)
        {
            var summary = (await _uow.UserAiSummaryRepository.GetAsync(s => s.UserId == userId)).FirstOrDefault();
            if (summary == null) return null;

            var top3UnisInfo = string.IsNullOrEmpty(summary.Top3Recommendations) 
                ? new List<RecommendedUniInfo>() 
                : JsonSerializer.Deserialize<List<RecommendedUniInfo>>(summary.Top3Recommendations) ?? new List<RecommendedUniInfo>();

            var next5UnisInfo = string.IsNullOrEmpty(summary.Next5Recommendations) 
                ? new List<RecommendedUniInfo>() 
                : JsonSerializer.Deserialize<List<RecommendedUniInfo>>(summary.Next5Recommendations) ?? new List<RecommendedUniInfo>();

            var allUniIds = top3UnisInfo.Select(u => Guid.TryParse(u.universityId, out var g) ? g : Guid.Empty).Where(g => g != Guid.Empty)
                .Concat(next5UnisInfo.Select(u => Guid.TryParse(u.universityId, out var g) ? g : Guid.Empty).Where(g => g != Guid.Empty))
                .Distinct().ToList();

            var allMajorIds = top3UnisInfo.SelectMany(u => u.majorIds).Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty).Where(g => g != Guid.Empty)
                .Concat(next5UnisInfo.SelectMany(u => u.majorIds).Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty).Where(g => g != Guid.Empty))
                .Distinct().ToList();

            var loadedUnis = (await _uow.UniversityRepository.GetAsync(u => allUniIds.Contains(u.UniversityId))).ToList();
            var loadedMajors = (await _uow.MajorRepository.GetAsync(m => allMajorIds.Contains(m.MajorId))).ToList();

            return MapToResponseDto(summary.Id, userId, summary.SummaryText, summary.CreatedAt, top3UnisInfo, next5UnisInfo, loadedUnis, loadedMajors);
        }

        public async Task<UserAiSummaryResponseDto> EvaluateChatAiOverallAsync(Guid userId)
        {
            // 1. Fetch Chat AI category
            var chatCategory = (await _uow.QuestionCategoryRepository.GetAsync(c => c.IsChatAi)).FirstOrDefault();
            if (chatCategory == null)
            {
                throw new Exception("Không tìm thấy chuyên mục Chat AI.");
            }

            // 2. Fetch active questions in Chat AI category
            var activeQuestions = (await _uow.QuestionRepository.GetAsync(q => q.CategoryId == chatCategory.Id && q.IsActice == StatusEnum.Yes)).ToList();
            if (!activeQuestions.Any())
            {
                throw new Exception("Chuyên mục Chat AI chưa được cấu hình câu hỏi nào.");
            }

            // 3. Fetch user answers
            var questionIds = activeQuestions.Select(q => q.Id).ToList();
            var userAnswers = (await _uow.UserAnswerRepository.GetAsync(a => a.UserId == userId && questionIds.Contains(a.QuestionId))).ToList();

            // 4. Verify that all questions are answered
            if (userAnswers.Count < activeQuestions.Count)
            {
                throw new Exception("Bạn cần trả lời đầy đủ tất cả câu hỏi trong Chat AI để nhận nhận xét tổng kết và đề xuất trường đại học.");
            }

            // 4.5. Check cache: if summary already exists and answers haven't changed since then
            var cachedSumm = (await _uow.UserAiSummaryRepository.GetAsync(s => s.UserId == userId)).FirstOrDefault();
            if (cachedSumm != null && userAnswers.Any())
            {
                var maxAnsweredAt = userAnswers.Max(a => a.AnsweredAt);
                if (cachedSumm.CreatedAt >= maxAnsweredAt)
                {
                    var cachedSummary = await GetOverallSummaryAsync(userId);
                    if (cachedSummary != null)
                    {
                        return cachedSummary;
                    }
                }
            }

            // 5. Construct prompt details
            var sb = new StringBuilder();
            sb.AppendLine($"Chuyên mục: {chatCategory.Name}");
            for (int i = 0; i < activeQuestions.Count; i++)
            {
                var q = activeQuestions[i];
                var ans = userAnswers.FirstOrDefault(a => a.QuestionId == q.Id);
                sb.AppendLine($"  {i + 1}. Câu hỏi: {q.Content}");
                sb.AppendLine($"     Câu trả lời: {ans?.Answer}");
            }

            // 6. Load all universities, majors and university-majors
            var universities = (await _uow.UniversityRepository.GetAsync()).ToList();
            var universityMajors = (await _uow.UniversityMajorRepository.GetAsync()).ToList();
            var majors = (await _uow.MajorRepository.GetAsync()).ToList();

            var uniListSb = new StringBuilder();
            foreach (var uni in universities)
            {
                var uniMajorIds = universityMajors.Where(um => um.UniversityId == uni.UniversityId).Select(um => um.MajorId).ToList();
                var uniMajors = majors.Where(m => uniMajorIds.Contains(m.MajorId)).ToList();
                var majorStrings = uniMajors.Select(m => $"[ID Ngành: {m.MajorId}] {m.Name ?? "Chưa đặt tên"}").ToList();
                var majorListStr = majorStrings.Any() ? string.Join(", ", majorStrings) : "Không có ngành học nào được đăng ký";

                uniListSb.AppendLine($"- [ID Trường: {uni.UniversityId}] Tên: {uni.Name} ({uni.ShortName}), Địa chỉ: {uni.Location}, Xếp hạng: {uni.Ranking}");
                uniListSb.AppendLine($"  Các ngành học đào tạo: {majorListStr}");
            }

            // 7. Build prompt
            var prompt = $@"
Bạn là một chuyên gia tư vấn tuyển sinh và định hướng nghề nghiệp hàng đầu.
Hãy phân tích toàn bộ câu trả lời của người dùng dưới đây trong cuộc trò chuyện Chat AI để đưa ra đánh giá tổng quan và đề xuất trường đại học cùng ngành học phù hợp nhất cho họ.

Dữ liệu câu trả lời của người dùng:
{sb.ToString()}

Danh sách các trường đại học hiện có và các ngành học đào tạo tương ứng trong hệ thống của chúng tôi:
{uniListSb.ToString()}

Yêu cầu:
1. Đưa ra nhận xét, đánh giá tổng quan (nhận xét chung) về thế mạnh, sở thích nghề nghiệp, định hướng tuyển sinh của người dùng dựa trên tất cả câu trả lời của họ.
2. Từ danh sách các trường đại học được cung cấp ở trên, hãy chọn ra:
   - 3 trường đại học phù hợp nhất (danh sách 'top3' chứa các đề xuất tương ứng).
   - 5 trường đại học phù hợp nhì (danh sách 'next5' chứa các đề xuất tương ứng).
   *Chú ý: Đối với mỗi trường đại học được chọn, hãy gợi ý từ 1-2 ngành học đào tạo phù hợp nhất của chính trường đó (sử dụng đúng ID Ngành được cung cấp dưới trường đó) và đánh giá tỷ lệ phần trăm độ phù hợp của trường này với người dùng (là số nguyên từ 0 đến 100, ví dụ 80, 75, lưu vào trường 'matchPercentage').*
   *Tuyệt đối chỉ chọn từ danh sách ID Trường và ID Ngành học được cung cấp ở trên. Không tự ý bịa ra ID nằm ngoài danh sách.*
3. Trả về kết quả dưới dạng JSON hợp lệ khớp chính xác với cấu trúc sau:
{{
  ""summaryText"": ""Nội dung nhận xét tổng quan bằng tiếng Việt... (dùng markdown nếu cần thiết)"",
  ""top3"": [
    {{
      ""universityId"": ""id-truong-1"",
      ""matchPercentage"": 85,
      ""majorIds"": [""id-nganh-1"", ""id-nganh-2""]
    }}
  ],
  ""next5"": [
    {{
      ""universityId"": ""id-truong-2"",
      ""matchPercentage"": 70,
      ""majorIds"": [""id-nganh-3""]
    }}
  ]
}}
Vui lòng không trả về bất kỳ văn bản nào khác ngoài khối JSON này.
";

            // 8. Request Gemini API in JSON mode
            var apiKeys = new List<string>();
            var k1 = _configuration["Gemini:ApiKey1"] ?? _configuration["Gemini:ApiKey"];
            var k2 = _configuration["Gemini:ApiKey2"];
            var k3 = _configuration["Gemini:ApiKey3"];

            if (!string.IsNullOrEmpty(k1)) apiKeys.Add(k1);
            if (!string.IsNullOrEmpty(k2)) apiKeys.Add(k2);
            if (!string.IsNullOrEmpty(k3)) apiKeys.Add(k3);

            if (!apiKeys.Any())
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
            HttpResponseMessage response = null!;
            bool isSuccess = false;
            string lastError = "";

            var models = new List<string>
            {
                "gemini-2.5-flash",
                "gemini-3.5-flash",
                "gemini-2.5-flash-lite",
                "gemini-3-flash-preview",
                "gemini-2.0-flash",
                "gemini-2.0-flash-lite",
                "gemini-2.5-pro",
                "gemini-3.1-pro-preview",
                "gemini-3.1-flash-lite"
            };

            foreach (var model in models)
            {
                foreach (var key in apiKeys)
                {
                    try
                    {
                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        response = await _httpClient.PostAsync(
                            $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={key}",
                            content);
                        if (response.IsSuccessStatusCode)
                        {
                            isSuccess = true;
                            break;
                        }
                        else
                        {
                            var errBody = await response.Content.ReadAsStringAsync();
                            lastError = $"Model: {model}, Status: {response.StatusCode}, Body: {errBody}";
                        }
                    }
                    catch (Exception ex)
                    {
                        lastError = $"Model: {model}, Error: {ex.Message}";
                    }
                }

                if (isSuccess)
                {
                    break;
                }
            }

            if (!isSuccess)
            {
                throw new Exception($"Không thể thực hiện cuộc gọi đến Gemini API với các API Key và Model hiện có. Lỗi gần nhất: {lastError}");
            }

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

            // 9. Parse JSON response
            var geminiResult = JsonSerializer.Deserialize<GeminiSummaryResponse>(rawAnswerJson);
            if (geminiResult == null)
            {
                throw new Exception("Lỗi phân tích kết quả phản hồi của AI.");
            }

            var top3Str = JsonSerializer.Serialize(geminiResult.top3);
            var next5Str = JsonSerializer.Serialize(geminiResult.next5);

            // 10. Save or Update in database
            var existing = (await _uow.UserAiSummaryRepository.GetAsync(s => s.UserId == userId)).FirstOrDefault();
            Guid entityId;
            if (existing != null)
            {
                entityId = existing.Id;
                existing.SummaryText = geminiResult.summaryText;
                existing.Top3Recommendations = top3Str;
                existing.Next5Recommendations = next5Str;
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
                    Top3Recommendations = top3Str,
                    Next5Recommendations = next5Str,
                    CreatedAt = DateTime.UtcNow
                };
                await _uow.UserAiSummaryRepository.AddAsync(newSummary);
            }

            await _uow.SaveAsync();

            // 11. Map detailed data
            return MapToResponseDto(entityId, userId, geminiResult.summaryText, DateTime.UtcNow, geminiResult.top3, geminiResult.next5, universities, majors);
        }

        private UserAiSummaryResponseDto MapToResponseDto(
            Guid id,
            Guid userId,
            string summaryText,
            DateTime createdAt,
            List<RecommendedUniInfo> top3Info,
            List<RecommendedUniInfo> next5Info,
            List<University> loadedUnis,
            List<Major> loadedMajors)
        {
            var top3Unis = top3Info
                .Select(info => {
                    if (!Guid.TryParse(info.universityId, out var uniGuid)) return null;
                    var uni = loadedUnis.FirstOrDefault(u => u.UniversityId == uniGuid);
                    if (uni == null) return null;

                    var suitableMajors = info.majorIds
                        .Select(mid => Guid.TryParse(mid, out var mGuid) ? mGuid : Guid.Empty)
                        .Select(mGuid => loadedMajors.FirstOrDefault(m => m.MajorId == mGuid))
                        .Where(m => m != null)
                        .Select(m => new MajorDto {
                            MajorId = m!.MajorId,
                            Name = m.Name ?? string.Empty,
                            Description = m.Description ?? string.Empty
                        })
                        .ToList();

                    return new RecommendedUniversityDto {
                        UniversityId = uni.UniversityId,
                        Name = uni.Name,
                        ShortName = uni.ShortName,
                        Location = uni.Location,
                        Ranking = uni.Ranking,
                        Avatar = uni.Avatar,
                        MatchPercentage = info.matchPercentage,
                        SuitableMajors = suitableMajors
                    };
                })
                .Where(u => u != null)
                .Select(u => u!)
                .ToList();

            var next5Unis = next5Info
                .Select(info => {
                    if (!Guid.TryParse(info.universityId, out var uniGuid)) return null;
                    var uni = loadedUnis.FirstOrDefault(u => u.UniversityId == uniGuid);
                    if (uni == null) return null;

                    var suitableMajors = info.majorIds
                        .Select(mid => Guid.TryParse(mid, out var mGuid) ? mGuid : Guid.Empty)
                        .Select(mGuid => loadedMajors.FirstOrDefault(m => m.MajorId == mGuid))
                        .Where(m => m != null)
                        .Select(m => new MajorDto {
                            MajorId = m!.MajorId,
                            Name = m.Name ?? string.Empty,
                            Description = m.Description ?? string.Empty
                        })
                        .ToList();

                    return new RecommendedUniversityDto {
                        UniversityId = uni.UniversityId,
                        Name = uni.Name,
                        ShortName = uni.ShortName,
                        Location = uni.Location,
                        Ranking = uni.Ranking,
                        Avatar = uni.Avatar,
                        MatchPercentage = info.matchPercentage,
                        SuitableMajors = suitableMajors
                    };
                })
                .Where(u => u != null)
                .Select(u => u!)
                .ToList();

            return new UserAiSummaryResponseDto
            {
                Id = id,
                UserId = userId,
                SummaryText = summaryText,
                CreatedAt = createdAt,
                Top3Universities = top3Unis,
                Next5Universities = next5Unis
            };
        }

        private class GeminiSummaryResponse
        {
            public string summaryText { get; set; } = string.Empty;
            public List<RecommendedUniInfo> top3 { get; set; } = new List<RecommendedUniInfo>();
            public List<RecommendedUniInfo> next5 { get; set; } = new List<RecommendedUniInfo>();
        }

        private class RecommendedUniInfo
        {
            public string universityId { get; set; } = string.Empty;
            public List<string> majorIds { get; set; } = new List<string>();
            public int matchPercentage { get; set; }
        }
    }
}
