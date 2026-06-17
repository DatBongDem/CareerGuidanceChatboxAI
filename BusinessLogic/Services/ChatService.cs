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

            // 4. Determine current state
            // Find the first question that hasn't been answered yet
            var currentQuestion = chatQuestions.FirstOrDefault(q => !userAnswers.Any(a => a.QuestionId == q.Id));

            // If there is still an unanswered question and user has provided a reply, save it
            if (currentQuestion != null && !string.IsNullOrWhiteSpace(userMessage))
            {
                // Double check if answer already exists to prevent duplicate
                var existing = await _unitOfWork.UserAnswerRepository.GetAsync(a => a.UserId == userId && a.QuestionId == currentQuestion.Id);
                if (!existing.Any())
                {
                    var newAnswer = new UserAnswer
                    {
                        UserAnswerId = Guid.NewGuid(),
                        UserId = userId,
                        QuestionId = currentQuestion.Id,
                        Answer = userMessage,
                        AnsweredAt = DateTime.UtcNow
                    };
                    await _unitOfWork.UserAnswerRepository.AddAsync(newAnswer);
                    await _unitOfWork.SaveAsync();

                    // Update local lists
                    userAnswers.Add(newAnswer);
                }

                // Advance to the next question
                currentQuestion = chatQuestions.FirstOrDefault(q => !userAnswers.Any(a => a.QuestionId == q.Id));
            }

            // 5. If all questions are answered, generate summary and return completed response
            if (currentQuestion == null)
            {
                // Evaluate UserAiSummary
                var summary = await _userAiSummaryService.EvaluateChatAiOverallAsync(userId);

                // Let AI say a warm ending message presenting the completion
                var endingPrompt = $@"
Bạn là một chuyên gia tư vấn tuyển sinh và định hướng nghề nghiệp AI thân thiện.
Người dùng vừa hoàn thành tất cả các câu hỏi của cuộc khảo sát.
Hãy tạo một lời thoại kết thúc cuộc trò chuyện thật tự nhiên, thân thiện bằng tiếng Việt, thông báo rằng họ đã hoàn thành cuộc khảo sát hướng nghiệp và chúc mừng họ đã nhận được kết quả nhận xét/đề xuất trường học.
Tránh đưa ra văn bản thừa thãi, trả lời ngắn gọn và truyền cảm hứng.
";
                var endingMessage = await CallGeminiSimpleAsync(endingPrompt);

                return new GuidedChatResponse
                {
                    Message = endingMessage,
                    IsCompleted = true,
                    Summary = summary
                };
            }

            // 6. Otherwise, ask the next question
            // We want to ask currentQuestion.
            // Let's generate a conversational question using Gemini, showing the history (if any) to make it feel natural.
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

Câu hỏi tiếp theo bạn CẦN hỏi: ""{currentQuestion.Content}""

Hãy tạo một phản hồi tự nhiên bằng tiếng Việt:
1. Nhận xét ngắn gọn, đồng cảm hoặc phản hồi thân thiện về câu trả lời gần đây nhất của người dùng (nếu có lịch sử).
2. Chuyển ý tự nhiên và đặt câu hỏi tiếp theo ở trên cho người dùng một cách duyên dáng.
Không tự ý đổi nội dung cốt lõi của câu hỏi tiếp theo, chỉ trang trí lời thoại xung quanh nó để cuộc hội thoại tự nhiên hơn.
Trả lời súc tích, ngắn gọn, phù hợp với định dạng trò chuyện chat.
";

            var aiResponse = await CallGeminiSimpleAsync(guidedPrompt);

            return new GuidedChatResponse
            {
                Message = aiResponse,
                IsCompleted = false
            };
        }

        private async Task<string> CallGeminiSimpleAsync(string prompt)
        {
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

            return answer?.Trim() ?? string.Empty;
        }
    }
}
