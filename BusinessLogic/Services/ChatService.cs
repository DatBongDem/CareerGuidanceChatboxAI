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

        public async Task<GuidedChatResponse> ContinueGuidedChatAsync(Guid userId, Guid? sessionId, string? userMessage)
        {
            // 1. Get Chat AI category
            var chatCategory = (await _unitOfWork.QuestionCategoryRepository.GetAsync(c => c.IsChatAi)).FirstOrDefault();
            if (chatCategory == null)
            {
                return new GuidedChatResponse
                {
                    Message = "Hệ thống chưa thiết lập chuyên mục câu hỏi Chat AI. Vui lòng liên hệ Admin.",
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
                };
            }

            // 3. Retrieve or create Session
            ChatAiSession? session = null;
            if (sessionId.HasValue && sessionId.Value != Guid.Empty)
            {
                session = await _unitOfWork.ChatAiSessionRepository.GetByIdAsync(sessionId.Value);
            }

            if (session == null || session.UserId != userId)
            {
                var sessionCount = (await _unitOfWork.ChatAiSessionRepository.GetAsync(s => s.UserId == userId)).Count();
                session = new ChatAiSession
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Name = $"Phiên chat {sessionCount + 1}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _unitOfWork.ChatAiSessionRepository.AddAsync(session);
                await _unitOfWork.SaveAsync();

            }

            // 4. Load session answers
            var userAnswers = (await _unitOfWork.ChatAiAnswerRepository.GetAsync(
                a => a.SessionId == session.Id
            )).ToList();

            // 5. If this is the start of the chat in this session and user message is empty
            if (userAnswers.Count == 0 && string.IsNullOrWhiteSpace(userMessage))
            {
                return new GuidedChatResponse
                {
                    SessionId = session.Id,
                    Evaluation = "",
                    Message = "Xin chào! Tôi là Trợ lý Hướng nghiệp AI. Hãy chia sẻ để tôi có thể tìm ngành học và trường đại học phù hợp nhất với bạn nhé!",
                    HasEnoughInfo = false,
                    Summary = null,
                    NextQuestionContent = chatQuestions.First().Content
                };
            }

            // Otherwise, process user message
            var activeQuestion = chatQuestions.FirstOrDefault(q => !userAnswers.Any(a => a.QuestionId == q.Id));

            if (activeQuestion != null && !string.IsNullOrWhiteSpace(userMessage))
            {
                var existing = await _unitOfWork.ChatAiAnswerRepository.GetAsync(
                    a => a.SessionId == session.Id && a.QuestionId == activeQuestion.Id
                );
                if (!existing.Any())
                {
                    var newAnswer = new ChatAiAnswer
                    {
                        Id = Guid.NewGuid(),
                        SessionId = session.Id,
                        QuestionId = activeQuestion.Id,
                        Answer = userMessage,
                        AnsweredAt = DateTime.UtcNow
                    };
                    await _unitOfWork.ChatAiAnswerRepository.AddAsync(newAnswer);
                    await _unitOfWork.SaveAsync();

                    userAnswers.Add(newAnswer);
                }
            }

            // Find last answered question
            var lastAnsweredQuestion = chatQuestions
                .Where(q => userAnswers.Any(a => a.QuestionId == q.Id))
                .OrderBy(q => q.DisplayOrder)
                .LastOrDefault();
            var lastAnswerEntity = userAnswers.FirstOrDefault(a => a.QuestionId == lastAnsweredQuestion?.Id);
            var lastAnswer = lastAnswerEntity?.Answer;

            // Determine next question
            var nextQuestion = chatQuestions.FirstOrDefault(q => !userAnswers.Any(a => a.QuestionId == q.Id));

            // 6. If all questions answered, generate overall summary and complete session
            if (nextQuestion == null)
            {
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

                var historySb = new StringBuilder();
                historySb.AppendLine($"Chuyên mục: {chatCategory.Name}");
                for (int i = 0; i < chatQuestions.Count; i++)
                {
                    var q = chatQuestions[i];
                    var ans = userAnswers.FirstOrDefault(a => a.QuestionId == q.Id);
                    historySb.AppendLine($"  {i + 1}. Câu hỏi: {q.Content}");
                    historySb.AppendLine($"     Câu trả lời: {ans?.Answer}");
                }

                var endingPrompt = $@"
Bạn là một chuyên gia tư vấn tuyển sinh và định hướng nghề nghiệp AI thân thiện.
Người dùng vừa hoàn thành câu hỏi cuối cùng của cuộc khảo sát Chat AI.
Dữ liệu câu trả lời của người dùng:
{historySb.ToString()}

Danh sách các trường đại học hiện có và các ngành học đào tạo tương ứng trong hệ thống của chúng tôi:
{uniListSb.ToString()}

Nhiệm vụ của bạn là:
1. Đưa ra một câu nhận xét, đánh giá ngắn gọn và có ích về câu trả lời cuối cùng này của người dùng (lưu vào trường 'evaluation').
2. Tạo một lời thoại kết thúc cuộc trò chuyện thật tự nhiên, thân thiện bằng tiếng Việt.
3. Từ danh sách các trường đại học được cung cấp ở trên, hãy chọn ra các trường đại học phù hợp nhất (lưu vào danh sách 'recommendations' thuộc 'summary').

Trả về kết quả dưới dạng JSON có cấu trúc như sau:
{{
  ""evaluation"": ""Đánh giá..."",
  ""message"": ""Lời thoại chúc mừng..."",
  ""summary"": {{
     ""summaryText"": ""Nhận xét tổng quan..."",
     ""recommendations"": [
       {{ ""universityId"": ""id"", ""matchPercentage"": 85, ""majorIds"": [""id1""] }}
     ]
  }}
}}
";
                var parsedResult = await CallGeminiJsonAsync<GeminiGuidedChatResponse>(endingPrompt);

                if (lastAnswerEntity != null && parsedResult != null)
                {
                    lastAnswerEntity.Evaluation = parsedResult.evaluation ?? string.Empty;
                    await _unitOfWork.ChatAiAnswerRepository.UpdateAsync(lastAnswerEntity);
                }

                ChatAiSummaryResponseDto? finalSummaryDto = null;
                if (parsedResult?.summary != null)
                {
                    var recommendationsJson = JsonSerializer.Serialize(parsedResult.summary.recommendations);
                    var existingSummary = (await _unitOfWork.ChatAiSummaryRepository.GetAsync(
                        s => s.SessionId == session.Id
                    )).FirstOrDefault();

                    if (existingSummary != null)
                    {
                        existingSummary.SummaryText = parsedResult.summary.summaryText;
                        existingSummary.Recommendations = recommendationsJson;
                        existingSummary.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.ChatAiSummaryRepository.UpdateAsync(existingSummary);

                        finalSummaryDto = MapToSummaryDto(
                            session.Id,
                            parsedResult.summary.summaryText,
                            parsedResult.summary.recommendations,
                            universities,
                            majors
                        );
                        finalSummaryDto.Id = existingSummary.Id;
                        finalSummaryDto.CreatedAt = existingSummary.CreatedAt;
                    }
                    else
                    {
                        var newSummary = new ChatAiSummary
                        {
                            Id = Guid.NewGuid(),
                            SessionId = session.Id,
                            SummaryText = parsedResult.summary.summaryText,
                            Recommendations = recommendationsJson,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _unitOfWork.ChatAiSummaryRepository.AddAsync(newSummary);

                        finalSummaryDto = MapToSummaryDto(
                            session.Id,
                            parsedResult.summary.summaryText,
                            parsedResult.summary.recommendations,
                            universities,
                            majors
                        );
                        finalSummaryDto.Id = newSummary.Id;
                        finalSummaryDto.CreatedAt = newSummary.CreatedAt;
                    }
                }

                session.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.ChatAiSessionRepository.UpdateAsync(session);
                await _unitOfWork.SaveAsync();

                return new GuidedChatResponse
                {
                    SessionId = session.Id,
                    Evaluation = parsedResult?.evaluation ?? string.Empty,
                    Message = parsedResult?.message ?? string.Empty,
                    HasEnoughInfo = true,
                    Summary = finalSummaryDto,
                    NextQuestionContent = null
                };
            }

            // Load universities, majors for context
            var universitiesList = (await _unitOfWork.UniversityRepository.GetAsync()).ToList();
            var universityMajorsList = (await _unitOfWork.UniversityMajorRepository.GetAsync()).ToList();
            var majorsList = (await _unitOfWork.MajorRepository.GetAsync()).ToList();

            var uniListSbContext = new StringBuilder();
            foreach (var uni in universitiesList)
            {
                var uniMajorIds = universityMajorsList.Where(um => um.UniversityId == uni.UniversityId).Select(um => um.MajorId).ToList();
                var uniMajors = majorsList.Where(m => uniMajorIds.Contains(m.MajorId)).ToList();
                var majorStrings = uniMajors.Select(m => $"[ID Ngành: {m.MajorId}] {m.Name ?? "Chưa đặt tên"}").ToList();
                var majorListStr = majorStrings.Any() ? string.Join(", ", majorStrings) : "Không có ngành học nào được đăng ký";

                uniListSbContext.AppendLine($"- [ID Trường: {uni.UniversityId}] Tên: {uni.Name} ({uni.ShortName}), Địa chỉ: {uni.Location}, Xếp hạng: {uni.Ranking}");
                uniListSbContext.AppendLine($"  Các ngành học đào tạo: {majorListStr}");
            }

            var historySbContext = new StringBuilder();
            var answeredQuestions = chatQuestions.Where(q => userAnswers.Any(a => a.QuestionId == q.Id)).ToList();
            if (answeredQuestions.Any())
            {
                historySbContext.AppendLine("Lịch sử trò chuyện và đánh giá trước đó:");
                foreach (var q in answeredQuestions)
                {
                    var ans = userAnswers.FirstOrDefault(a => a.QuestionId == q.Id);
                    historySbContext.AppendLine($"- AI hỏi: {q.Content}");
                    historySbContext.AppendLine($"  Người dùng trả lời: {ans?.Answer}");
                    if (!string.IsNullOrEmpty(ans?.Evaluation))
                    {
                        historySbContext.AppendLine($"  Đánh giá của AI cho câu trả lời này: {ans.Evaluation}");
                    }
                }
            }

            var guidedPrompt = $@"
Bạn là một chuyên gia tư vấn hướng nghiệp AI thân thiện. 
Nhiệm vụ của bạn là dẫn dắt cuộc trò chuyện một cách tự nhiên và đặt câu hỏi tiếp theo: ""{nextQuestion.Content}""

{historySbContext.ToString()}

Câu hỏi vừa trả lời: ""{lastAnsweredQuestion?.Content}""
Câu trả lời của người dùng: ""{lastAnswer}""

Danh sách các trường đại học:
{uniListSbContext.ToString()}

Quy tắc trò chuyện và đánh giá:
1. Bạn phải đọc kỹ lịch sử trò chuyện và các đánh giá trước đó (nếu có) trong phần ngữ cảnh để hiểu rõ tính cách, năng lực và sở thích của người dùng.
2. Lời thoại dẫn dắt (trường ""message"") phải liên kết một cách mượt mà và thông minh với câu trả lời vừa rồi của người dùng và bối cảnh trước đó. Tuyệt đối không được đưa ra phản hồi gượng gạo, rập khuôn hoặc chuyển chủ đề một cách đột ngột.
3. Đặt ""hasEnoughInfo"" thành true và cung cấp đề xuất sơ bộ trong ""summary"" (dù tỉ lệ % phù hợp có thể thấp từ 30% - 50%) ngay khi câu trả lời của người dùng chứa bất kỳ thông tin cụ thể hữu ích nào về sở thích, môn học thế mạnh, tính cách, kỹ năng, hoặc định hướng nghề nghiệp.
4. Đặt ""hasEnoughInfo"" thành false và ""summary"" thành null nếu người dùng chưa cung cấp bất kỳ thông tin cụ thể nào hỗ trợ định hướng nghề nghiệp (ví dụ: mới chỉ chào hỏi xã giao hoặc câu trả lời không mang tính thông tin định hướng nào).

Trả về dạng JSON:
{{
  ""evaluation"": ""Đánh giá ngắn gọn, sâu sắc về câu trả lời vừa rồi"",
  ""message"": ""Lời thoại dẫn dắt mượt mà kết nối lịch sử và câu hỏi tiếp theo"",
  ""hasEnoughInfo"": true/false,
  ""summary"": null hoặc {{
     ""summaryText"": ""Nhận xét tổng quan..."",
     ""recommendations"": [
       {{ ""universityId"": ""guid-cua-truong-dai-hoc"", ""matchPercentage"": 85, ""majorIds"": [""guid-nganh-1"", ""guid-nganh-2""] }}
     ]
  }}
}}
";

            var parsedGuidedResult = await CallGeminiJsonAsync<GeminiGuidedChatResponse>(guidedPrompt);

            if (lastAnswerEntity != null && parsedGuidedResult != null)
            {
                lastAnswerEntity.Evaluation = parsedGuidedResult.evaluation ?? string.Empty;
                await _unitOfWork.ChatAiAnswerRepository.UpdateAsync(lastAnswerEntity);
            }

            bool enoughInfo = parsedGuidedResult != null && parsedGuidedResult.hasEnoughInfo;

            ChatAiSummaryResponseDto? summaryDto = null;
            if (enoughInfo && parsedGuidedResult?.summary != null)
            {
                var recommendationsJson = JsonSerializer.Serialize(parsedGuidedResult.summary.recommendations);
                var existingSummary = (await _unitOfWork.ChatAiSummaryRepository.GetAsync(
                    s => s.SessionId == session.Id
                )).FirstOrDefault();

                if (existingSummary != null)
                {
                    existingSummary.SummaryText = parsedGuidedResult.summary.summaryText;
                    existingSummary.Recommendations = recommendationsJson;
                    existingSummary.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.ChatAiSummaryRepository.UpdateAsync(existingSummary);

                    summaryDto = MapToSummaryDto(
                        session.Id,
                        parsedGuidedResult.summary.summaryText,
                        parsedGuidedResult.summary.recommendations,
                        universitiesList,
                        majorsList
                    );
                    summaryDto.Id = existingSummary.Id;
                    summaryDto.CreatedAt = existingSummary.CreatedAt;
                }
                else
                {
                    var newSummary = new ChatAiSummary
                    {
                        Id = Guid.NewGuid(),
                        SessionId = session.Id,
                        SummaryText = parsedGuidedResult.summary.summaryText,
                        Recommendations = recommendationsJson,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.ChatAiSummaryRepository.AddAsync(newSummary);

                    summaryDto = MapToSummaryDto(
                        session.Id,
                        parsedGuidedResult.summary.summaryText,
                        parsedGuidedResult.summary.recommendations,
                        universitiesList,
                        majorsList
                    );
                    summaryDto.Id = newSummary.Id;
                    summaryDto.CreatedAt = newSummary.CreatedAt;
                }
            }

            session.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.ChatAiSessionRepository.UpdateAsync(session);
            await _unitOfWork.SaveAsync();

            return new GuidedChatResponse
            {
                SessionId = session.Id,
                Evaluation = parsedGuidedResult?.evaluation ?? string.Empty,
                Message = parsedGuidedResult?.message ?? string.Empty,
                HasEnoughInfo = enoughInfo,
                Summary = summaryDto,
                NextQuestionContent = nextQuestion?.Content
            };
        }

        private ChatAiSummaryResponseDto MapToSummaryDto(
            Guid sessionId,
            string summaryText,
            List<RecommendedUniInfo> recs,
            List<University> loadedUnis,
            List<Major> loadedMajors)
        {
            var recommendations = recs
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

            return new ChatAiSummaryResponseDto
            {
                Id = Guid.Empty,
                SessionId = sessionId,
                SummaryText = summaryText,
                CreatedAt = DateTime.UtcNow,
                Recommendations = recommendations
            };
        }

        public async Task<IEnumerable<ChatAiSessionDto>> GetUserChatSessionsAsync(Guid userId)
        {
            var sessions = await _unitOfWork.ChatAiSessionRepository.GetAsync(
                s => s.UserId == userId,
                orderBy: q => q.OrderByDescending(s => s.UpdatedAt)
            );

            return sessions.Select(s => new ChatAiSessionDto
            {
                Id = s.Id,
                Name = s.Name,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            });
        }

        public async Task<ChatAiSessionDetailDto?> GetChatSessionDetailAsync(Guid userId, Guid sessionId)
        {
            var session = await _unitOfWork.ChatAiSessionRepository.GetByIdAsync(sessionId);
            if (session == null || session.UserId != userId)
            {
                return null;
            }

            var answers = (await _unitOfWork.ChatAiAnswerRepository.GetAsync(
                a => a.SessionId == sessionId,
                includeProperties: "Question"
            )).OrderBy(a => a.AnsweredAt).ToList();

            ChatAiSummaryResponseDto? summaryDto = null;
            var summaryEntity = (await _unitOfWork.ChatAiSummaryRepository.GetAsync(
                s => s.SessionId == sessionId
            )).FirstOrDefault();

            if (summaryEntity != null)
            {
                var recs = string.IsNullOrEmpty(summaryEntity.Recommendations)
                    ? new List<RecommendedUniInfo>()
                    : JsonSerializer.Deserialize<List<RecommendedUniInfo>>(summaryEntity.Recommendations) ?? new List<RecommendedUniInfo>();

                var allUniIds = recs.Select(u => Guid.TryParse(u.universityId, out var g) ? g : Guid.Empty).Where(g => g != Guid.Empty).Distinct().ToList();
                var allMajorIds = recs.SelectMany(u => u.majorIds).Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty).Where(g => g != Guid.Empty).Distinct().ToList();

                var loadedUnis = (await _unitOfWork.UniversityRepository.GetAsync(u => allUniIds.Contains(u.UniversityId))).ToList();
                var loadedMajors = (await _unitOfWork.MajorRepository.GetAsync(m => allMajorIds.Contains(m.MajorId))).ToList();

                summaryDto = MapToSummaryDto(
                    sessionId,
                    summaryEntity.SummaryText,
                    recs,
                    loadedUnis,
                    loadedMajors
                );
                summaryDto.Id = summaryEntity.Id;
                summaryDto.CreatedAt = summaryEntity.CreatedAt;
            }

            var chatHistory = answers.Select(a => new ChatAiMessageDto
            {
                QuestionId = a.QuestionId,
                QuestionContent = a.Question?.Content ?? string.Empty,
                UserAnswer = a.Answer,
                Evaluation = a.Evaluation,
                AnsweredAt = a.AnsweredAt
            }).ToList();

            string? nextQuestionContent = null;
            var chatCategory = (await _unitOfWork.QuestionCategoryRepository.GetAsync(c => c.IsChatAi)).FirstOrDefault();
            if (chatCategory != null)
            {
                var chatQuestions = (await _unitOfWork.QuestionRepository.GetAsync(
                    q => q.CategoryId == chatCategory.Id && q.IsActice == StatusEnum.Yes
                )).OrderBy(q => q.DisplayOrder).ToList();

                var nextQuestion = chatQuestions.FirstOrDefault(q => !answers.Any(a => a.QuestionId == q.Id));
                if (nextQuestion != null)
                {
                    nextQuestionContent = nextQuestion.Content;
                }
            }

            return new ChatAiSessionDetailDto
            {
                Id = session.Id,
                Name = session.Name,
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt,
                Summary = summaryDto,
                ChatHistory = chatHistory,
                NextQuestionContent = nextQuestionContent
            };
        }

        public async Task<bool> DeleteChatSessionAsync(Guid userId, Guid sessionId)
        {
            var session = await _unitOfWork.ChatAiSessionRepository.GetByIdAsync(sessionId);
            if (session == null || session.UserId != userId)
            {
                return false;
            }

            await _unitOfWork.ChatAiSessionRepository.DeleteAsync(sessionId);
            await _unitOfWork.SaveAsync();
            return true;
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

            var rawAnswer = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return rawAnswer ?? string.Empty;
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

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<T>(rawAnswerJson, options);
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
            public List<RecommendedUniInfo> recommendations { get; set; } = new List<RecommendedUniInfo>();
        }

        private class RecommendedUniInfo
        {
            public string universityId { get; set; } = string.Empty;
            public List<string> majorIds { get; set; } = new List<string>();
            public int matchPercentage { get; set; }
        }
    }
}
