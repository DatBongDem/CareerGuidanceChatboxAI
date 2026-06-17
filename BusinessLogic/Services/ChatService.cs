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
                    IsCompleted = false
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
                    Summary = summary
                };
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

Hãy tạo một phản hồi tự nhiên bằng tiếng Việt và trả về dạng JSON:
1. Đưa ra một câu nhận xét, đánh giá ngắn gọn và có ích về câu trả lời vừa rồi của người dùng đối với câu hỏi ""{lastAnsweredQuestion?.Content}"" (lưu vào trường 'evaluation'). Nhận xét khoảng 30-50 từ, không dùng ký tự markdown như *, -, #.
2. Chuyển ý tự nhiên và đặt câu hỏi tiếp theo ở trên cho người dùng một cách duyên dáng (lưu vào trường 'message').

Trả về kết quả dưới dạng JSON có cấu trúc như sau:
{{
  ""evaluation"": ""Đánh giá của bạn về câu trả lời vừa rồi"",
  ""message"": ""Lời thoại dẫn dắt và câu hỏi tiếp theo""
}}
Vui lòng không trả về bất kỳ văn bản nào khác ngoài khối JSON này.
";

            var parsedGuidedResult = await CallGeminiJsonAsync<GeminiGuidedChatResponse>(guidedPrompt);

            return new GuidedChatResponse
            {
                Evaluation = parsedGuidedResult?.evaluation ?? string.Empty,
                Message = parsedGuidedResult?.message ?? string.Empty,
                IsCompleted = false
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
                "gemini-3.5-flash",
                "gemini-3-flash-preview",
                "gemini-2.5-pro",
                "gemini-2.5-flash",
                "gemini-2.5-flash-lite",
                "gemini-2.0-flash",
                "gemini-1.5-flash"
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
                "gemini-3.5-flash",
                "gemini-3-flash-preview",
                "gemini-2.5-pro",
                "gemini-2.5-flash",
                "gemini-2.5-flash-lite",
                "gemini-2.0-flash",
                "gemini-1.5-flash"
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

        private class GeminiGuidedChatResponse
        {
            public string evaluation { get; set; } = string.Empty;
            public string message { get; set; } = string.Empty;
        }
    }
}
