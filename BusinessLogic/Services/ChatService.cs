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
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IUserAiSummaryService _userAiSummaryService;

        public ChatService(
            IUnitOfWork unitOfWork,
            HttpClient httpClient,
            IConfiguration configuration,
            IUserAiSummaryService userAiSummaryService)
        {
            _unitOfWork = unitOfWork;
            _httpClient = httpClient;
            _configuration = configuration;
            _userAiSummaryService = userAiSummaryService;
        }

        public async Task<string> AskAIAsync(Guid userId, string question)
        {
            var prompt = $"""
        Bạn là chuyên gia tư vấn hướng nghiệp.

        Trả lời bằng tiếng Việt.
        Đưa ra lời khuyên rõ ràng.

        Câu hỏi:
        {question}
        """;

            var answer = await CallGeminiSimpleAsync(prompt);

            var history = new ChatHistory
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Question = question,
                Answer = answer,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ChatHistoryRepository.AddAsync(history);
            await _unitOfWork.SaveAsync();

            return answer;
        }

        public async Task<GuidedChatResponse> ContinueGuidedChatAsync(Guid userId, string? userMessage)
        {
            // 1. Get Chat AI category
            var chatCategory = (await _unitOfWork.QuestionCategoryRepository.GetAsync(c => c.IsChatAi)).FirstOrDefault();
            if (chatCategory == null)
            {
                return new GuidedChatResponse
                {
                    Message = "Hệ thống chưa thiết lập chuyên mục câu hỏi Chat AI. Vui lòng liên hệ Admin.",
                    IsCompleted = false
                };
            }

            // 2. Get active questions in Chat AI category
            var chatQuestions = (await _unitOfWork.QuestionRepository.GetAsync(
                q => q.CategoryId == chatCategory.Id && q.IsActice == StatusEnum.Yes
            )).OrderBy(q => q.DisplayOrder).ToList();

            if (!chatQuestions.Any())
            {
                return new GuidedChatResponse
                {
                    Message = "Chuyên mục Chat AI chưa có câu hỏi nào hoạt động.",
                    IsCompleted = false
                };
            }

            // 3. Load user answers for these questions
            var questionIds = chatQuestions.Select(q => q.Id).ToList();
            var userAnswers = (await _unitOfWork.UserAnswerRepository.GetAsync(
                a => a.UserId == userId && questionIds.Contains(a.QuestionId)
            )).ToList();

            // 4. If this is the start of the chat (no answers yet and no message sent)
            if (userAnswers.Count == 0 && string.IsNullOrWhiteSpace(userMessage))
            {
                var currentQuestion = chatQuestions.First();
                var startPrompt = $@"
Bạn là một chuyên gia tư vấn hướng nghiệp AI thân thiện.
Hãy bắt đầu cuộc trò chuyện bằng tiếng Việt một cách tự nhiên, chào mừng người dùng và đưa ra câu hỏi đầu tiên dưới đây để họ trả lời:
""{currentQuestion.Content}""
";
                var startMessage = await CallGeminiSimpleAsync(startPrompt);
                return new GuidedChatResponse
                {
                    Evaluation = "",
                    Message = startMessage,
                    IsCompleted = false,
                    HasEnoughInfo = false
                };
            }

            // Otherwise, we process the user's message
            // Find the current unanswered question
            var activeQuestion = chatQuestions.FirstOrDefault(q => !userAnswers.Any(a => a.QuestionId == q.Id));

            if (activeQuestion != null && !string.IsNullOrWhiteSpace(userMessage))
            {
                // Double check if answer already exists to prevent duplicate
                var existing = await _unitOfWork.UserAnswerRepository.GetAsync(a => a.UserId == userId && a.QuestionId == activeQuestion.Id);
                if (!existing.Any())
                {
                    var newAnswer = new UserAnswer
                    {
                        UserAnswerId = Guid.NewGuid(),
                        UserId = userId,
                        QuestionId = activeQuestion.Id,
                        Answer = userMessage,
                        AnsweredAt = DateTime.UtcNow
                    };
                    await _unitOfWork.UserAnswerRepository.AddAsync(newAnswer);
                    await _unitOfWork.SaveAsync();

                    // Update local lists
                    userAnswers.Add(newAnswer);
                }
            }

            // Find the question they just answered
            var lastAnsweredQuestion = chatQuestions
                .Where(q => userAnswers.Any(a => a.QuestionId == q.Id))
                .OrderBy(q => q.DisplayOrder)
                .LastOrDefault();
            var lastAnswer = userAnswers.FirstOrDefault(a => a.QuestionId == lastAnsweredQuestion?.Id)?.Answer;

            // Determine the next question
            var nextQuestion = chatQuestions.FirstOrDefault(q => !userAnswers.Any(a => a.QuestionId == q.Id));

            // 5. If all questions are answered, generate summary and return completed response
            if (nextQuestion == null)
            {
                // Evaluate UserAiSummary
                var summary = await _userAiSummaryService.EvaluateChatAiOverallAsync(userId);

                // Call Gemini to evaluate the last answer and say a warm ending message
                var endingPrompt = $@"
Bạn là một chuyên gia tư vấn tuyển sinh và định hướng nghề nghiệp AI thân thiện.
Người dùng vừa hoàn thành câu hỏi cuối cùng của cuộc khảo sát.
Câu hỏi vừa trả lời: ""{lastAnsweredQuestion?.Content}""
Câu trả lời của người dùng: ""{lastAnswer}""

Nhiệm vụ của bạn là:
1. Đưa ra một câu nhận xét, đánh giá ngắn gọn và có ích về câu trả lời cuối cùng này của người dùng (lưu vào trường 'evaluation'). Nhận xét khoảng 30-50 từ, không dùng ký tự định dạng markdown như *, -, #.
2. Tạo một lời thoại kết thúc cuộc trò chuyện thật tự nhiên, thân thiện bằng tiếng Việt, chúc mừng họ đã hoàn thành toàn bộ cuộc khảo sát hướng nghiệp và thông báo rằng họ có thể xem kết quả tổng hợp chi tiết bên dưới (lưu vào trường 'message').

Trả về kết quả dưới dạng JSON có cấu trúc như sau:
{{
  ""evaluation"": ""Đánh giá của bạn về câu trả lời cuối cùng vừa rồi"",
  ""message"": ""Lời thoại chúc mừng và kết thúc cuộc trò chuyện""
}}
Vui lòng không trả về bất kỳ văn bản nào khác ngoài khối JSON này.
";
                var parsedResult = await CallGeminiJsonAsync<GeminiGuidedChatResponse>(endingPrompt);

                return new GuidedChatResponse
                {
                    Evaluation = parsedResult?.evaluation ?? string.Empty,
                    Message = parsedResult?.message ?? string.Empty,
                    IsCompleted = true,
                    HasEnoughInfo = true,
                    Summary = summary
                };
            }

            // Load all universities, majors and university-majors for prompt context
            var universities = (await _unitOfWork.UniversityRepository.GetAsync()).ToList();
            var universityMajors = (await _unitOfWork.UniversityMajorRepository.GetAsync()).ToList();
            var majors = (await _unitOfWork.MajorRepository.GetAsync()).ToList();

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

            // 6. Otherwise, ask the next question
            // We want to ask nextQuestion, and evaluate the last answer
            var historySb = new StringBuilder();
            var answeredQuestions = chatQuestions.Where(q => userAnswers.Any(a => a.QuestionId == q.Id)).ToList();
            if (answeredQuestions.Any())
            {
                historySb.AppendLine("Lịch sử trò chuyện trước đó:");
                foreach (var q in answeredQuestions)
                {
                    var ans = userAnswers.FirstOrDefault(a => a.QuestionId == q.Id);
                    historySb.AppendLine($"AI hỏi: {q.Content}");
                    historySb.AppendLine($"Người dùng trả lời: {ans?.Answer}");
                }
            }

            var guidedPrompt = $@"
Bạn là một chuyên gia tư vấn hướng nghiệp AI thân thiện. 
Nhiệm vụ của bạn là dẫn dắt cuộc trò chuyện và đặt câu hỏi tiếp theo cho người dùng từ danh sách câu hỏi có sẵn.

{historySb.ToString()}

Câu hỏi vừa trả lời: ""{lastAnsweredQuestion?.Content}""
Câu trả lời của người dùng: ""{lastAnswer}""

Câu hỏi tiếp theo bạn CẦN hỏi: ""{nextQuestion.Content}""

Danh sách các trường đại học hiện có và các ngành học đào tạo tương ứng trong hệ thống của chúng tôi:
{uniListSb.ToString()}

Hãy tạo một phản hồi tự nhiên bằng tiếng Việt và trả về dạng JSON:
1. Đưa ra một câu nhận xét, đánh giá ngắn gọn và có ích về câu trả lời vừa rồi của người dùng đối với câu hỏi ""{lastAnsweredQuestion?.Content}"" (lưu vào trường 'evaluation'). Nhận xét khoảng 30-50 từ, không dùng ký tự markdown như *, -, #.
2. Chuyển ý tự nhiên và đặt câu hỏi tiếp theo ở trên cho người dùng một cách duyên dáng (lưu vào trường 'message').
3. Phân tích lịch sử cuộc trò chuyện và các câu trả lời hiện tại của người dùng. Hãy đánh giá xem thông tin đã **đủ** để đưa ra định hướng nghề nghiệp tổng quát và đề xuất các trường đại học phù hợp nhì/phù hợp nhất hay chưa:
   - Tuyệt đối KHÔNG được coi là đủ thông tin nếu người dùng mới chỉ chào hỏi xã giao, trả lời không liên quan, hoặc thông tin quá sơ sài. Chỉ coi là đủ thông tin khi người dùng đã cung cấp ít nhất một số thông tin cụ thể (chẳng hạn như: sở thích học tập, môn học thế mạnh hoặc mục tiêu nghề nghiệp thực tế).
   - Nếu chưa đủ thông tin, hãy thiết lập trường 'hasEnoughInfo' là false và 'summary' là null.
   - Nếu đã đủ thông tin cụ thể, hãy thiết lập trường 'hasEnoughInfo' là true. Đồng thời, từ danh sách các trường đại học được cung cấp ở trên, hãy chọn ra 3 trường phù hợp nhất (lưu vào 'top3') và 5 trường phù hợp nhì (lưu vào 'next5') kèm theo ngành học đề xuất tương ứng (dùng đúng ID Trường và ID Ngành học có trong hệ thống), đánh giá tỷ lệ phần trăm độ phù hợp của trường đại học này với người dùng (số nguyên từ 0 đến 100, ví dụ 80, 75, lưu vào 'matchPercentage'), và viết một nhận xét tổng quan súc tích bằng tiếng Việt về định hướng của họ (lưu vào 'summaryText' thuộc trường 'summary').
   *Lưu ý: Chỉ chọn trường và ngành thực sự tồn tại trong danh sách cung cấp ở trên. Không tự ý bịa ra ID.*

Trả về kết quả dưới dạng JSON có cấu trúc như sau:
{{
  ""evaluation"": ""Đánh giá của bạn về câu trả lời vừa rồi"",
  ""message"": ""Lời thoại dẫn dắt và câu hỏi tiếp theo"",
  ""hasEnoughInfo"": true hoặc false,
  ""summary"": null hoặc {{
     ""summaryText"": ""Nhận xét tổng quan bằng tiếng Việt... (dùng markdown nếu cần thiết)"",
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
}}
Vui lòng không trả về bất kỳ văn bản nào khác ngoài khối JSON này.
";

            var parsedGuidedResult = await CallGeminiJsonAsync<GeminiGuidedChatResponse>(guidedPrompt);

            UserAiSummaryResponseDto? summaryDto = null;
            bool enoughInfo = parsedGuidedResult != null && parsedGuidedResult.hasEnoughInfo;

            // Cưỡng chế bằng code C#: Nếu người dùng mới chỉ trả lời ít hơn 2 câu hỏi,
            // hoặc nội dung cung cấp chưa đủ tối thiểu để phân tích, ta buộc hasEnoughInfo = false
            if (userAnswers.Count < 2)
            {
                enoughInfo = false;
            }

            if (enoughInfo && parsedGuidedResult?.summary != null)
            {
                summaryDto = MapToResponseDto(
                    userId,
                    parsedGuidedResult.summary.summaryText,
                    parsedGuidedResult.summary.top3,
                    parsedGuidedResult.summary.next5,
                    universities,
                    majors
                );
            }

            return new GuidedChatResponse
            {
                Evaluation = parsedGuidedResult?.evaluation ?? string.Empty,
                Message = parsedGuidedResult?.message ?? string.Empty,
                IsCompleted = false,
                HasEnoughInfo = enoughInfo,
                Summary = summaryDto
            };
        }

        private UserAiSummaryResponseDto MapToResponseDto(
            Guid userId,
            string summaryText,
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
                Id = Guid.Empty,
                UserId = userId,
                SummaryText = summaryText,
                CreatedAt = DateTime.UtcNow,
                Top3Universities = top3Unis,
                Next5Universities = next5Unis
            };
        }

        private async Task<string> CallGeminiSimpleAsync(string prompt)
        {
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

            var answer = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return answer?.Trim() ?? string.Empty;
        }

        private async Task<T?> CallGeminiJsonAsync<T>(string prompt)
        {
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

            if (string.IsNullOrEmpty(rawAnswerJson)) return default;

            return JsonSerializer.Deserialize<T>(rawAnswerJson);
        }

        public async Task<bool> ResetGuidedChatAsync(Guid userId)
        {
            // 1. Get Chat AI category
            var chatCategory = (await _unitOfWork.QuestionCategoryRepository.GetAsync(c => c.IsChatAi)).FirstOrDefault();
            if (chatCategory == null)
            {
                return false;
            }

            // 2. Get active questions in Chat AI category
            var chatQuestions = (await _unitOfWork.QuestionRepository.GetAsync(
                q => q.CategoryId == chatCategory.Id
            )).ToList();

            if (!chatQuestions.Any())
            {
                return false;
            }

            // 3. Load user answers for these questions
            var questionIds = chatQuestions.Select(q => q.Id).ToList();
            var userAnswers = (await _unitOfWork.UserAnswerRepository.GetAsync(
                a => a.UserId == userId && questionIds.Contains(a.QuestionId)
            )).ToList();

            if (!userAnswers.Any())
            {
                // Vẫn dọn dẹp UserAiSummary nếu tồn tại
                var summaries = await _unitOfWork.UserAiSummaryRepository.GetAsync(s => s.UserId == userId);
                if (summaries.Any())
                {
                    foreach (var summary in summaries)
                    {
                        await _unitOfWork.UserAiSummaryRepository.DeleteAsync(summary.Id);
                    }
                    await _unitOfWork.SaveAsync();
                    return true;
                }
                return false;
            }

            // 4. Delete user answers
            foreach (var answer in userAnswers)
            {
                await _unitOfWork.UserAnswerRepository.DeleteAsync(answer.UserAnswerId);
            }

            // 5. Delete UserAiSummary associated with this user
            var userSummaries = await _unitOfWork.UserAiSummaryRepository.GetAsync(s => s.UserId == userId);
            foreach (var summary in userSummaries)
            {
                await _unitOfWork.UserAiSummaryRepository.DeleteAsync(summary.Id);
            }

            await _unitOfWork.SaveAsync();
            return true;
        }

        private class GeminiGuidedChatResponse
        {
            public string evaluation { get; set; } = string.Empty;
            public string message { get; set; } = string.Empty;
            public bool hasEnoughInfo { get; set; }
            public GeminiSummaryResponse? summary { get; set; }
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
