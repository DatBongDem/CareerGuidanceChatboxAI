using BusinessLogic.DTOs.Feedback;
using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FeedbackService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ==========================================
        // FEEDBACK QUESTIONS CRUD
        // ==========================================

        public async Task<IEnumerable<FeedbackQuestionDto>> GetAllQuestionsAsync()
        {
            var questions = await _unitOfWork.FeedbackQuestionRepository.GetAllAsync();
            return questions
                .OrderBy(q => q.Order)
                .Select(q => MapToQuestionDto(q));
        }

        public async Task<IEnumerable<FeedbackQuestionDto>> GetActiveQuestionsAsync()
        {
            var questions = await _unitOfWork.FeedbackQuestionRepository.GetAsync(
                filter: q => q.IsActive
            );
            return questions
                .OrderBy(q => q.Order)
                .Select(q => MapToQuestionDto(q));
        }

        public async Task<FeedbackQuestionDto> GetQuestionByIdAsync(Guid id)
        {
            var question = await _unitOfWork.FeedbackQuestionRepository.GetByIdAsync(id);
            if (question == null)
            {
                throw new ApplicationException("Không tìm thấy câu hỏi phản hồi.");
            }
            return MapToQuestionDto(question);
        }

        public async Task<FeedbackQuestionDto> CreateQuestionAsync(CreateFeedbackQuestionDto dto)
        {
            var question = new FeedbackQuestion
            {
                Id = Guid.NewGuid(),
                QuestionText = dto.QuestionText,
                QuestionType = dto.QuestionType,
                Options = dto.Options,
                Order = dto.Order,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.FeedbackQuestionRepository.AddAsync(question);
            await _unitOfWork.SaveAsync();

            return MapToQuestionDto(question);
        }

        public async Task<FeedbackQuestionDto> UpdateQuestionAsync(Guid id, CreateFeedbackQuestionDto dto)
        {
            var question = await _unitOfWork.FeedbackQuestionRepository.GetByIdAsync(id);
            if (question == null)
            {
                throw new ApplicationException("Không tìm thấy câu hỏi phản hồi.");
            }

            question.QuestionText = dto.QuestionText;
            question.QuestionType = dto.QuestionType;
            question.Options = dto.Options;
            question.Order = dto.Order;
            question.IsActive = dto.IsActive;

            await _unitOfWork.FeedbackQuestionRepository.UpdateAsync(question);
            await _unitOfWork.SaveAsync();

            return MapToQuestionDto(question);
        }

        public async Task DeleteQuestionAsync(Guid id)
        {
            var question = await _unitOfWork.FeedbackQuestionRepository.GetByIdAsync(id);
            if (question == null)
            {
                throw new ApplicationException("Không tìm thấy câu hỏi để xóa.");
            }

            // Check if there are any answers referencing this question
            var answers = await _unitOfWork.FeedbackAnswerRepository.GetAsync(filter: a => a.QuestionId == id);
            if (answers.Any())
            {
                // Soft-delete: instead of hard deleting, we deactivate it to maintain referential integrity
                question.IsActive = false;
                await _unitOfWork.FeedbackQuestionRepository.UpdateAsync(question);
            }
            else
            {
                await _unitOfWork.FeedbackQuestionRepository.DeleteAsync(id);
            }

            await _unitOfWork.SaveAsync();
        }

        // ==========================================
        // FEEDBACK RESPONSES
        // ==========================================

        public async Task<bool> SubmitFeedbackAsync(SubmitFeedbackDto dto, Guid? userId)
        {
            var responseId = Guid.NewGuid();
            var response = new FeedbackResponse
            {
                Id = responseId,
                UserId = userId,
                UserEmail = dto.UserEmail,
                UserFullName = dto.UserFullName,
                SubmittedAt = DateTime.UtcNow
            };

            // If user is logged in and details are not provided, try to fetch user info
            if (userId.HasValue && (string.IsNullOrEmpty(dto.UserEmail) || string.IsNullOrEmpty(dto.UserFullName)))
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId.Value);
                if (user != null)
                {
                    if (string.IsNullOrEmpty(dto.UserEmail)) response.UserEmail = user.Email;
                    if (string.IsNullOrEmpty(dto.UserFullName)) response.UserFullName = user.Username;
                }
            }

            await _unitOfWork.FeedbackResponseRepository.AddAsync(response);

            // Add answers
            foreach (var answerDto in dto.Answers)
            {
                // Verify question exists
                var question = await _unitOfWork.FeedbackQuestionRepository.GetByIdAsync(answerDto.QuestionId);
                if (question == null)
                {
                    throw new ApplicationException($"Không tìm thấy câu hỏi với ID {answerDto.QuestionId}");
                }

                var answer = new FeedbackAnswer
                {
                    Id = Guid.NewGuid(),
                    ResponseId = responseId,
                    QuestionId = answerDto.QuestionId,
                    AnswerText = answerDto.AnswerText
                };

                await _unitOfWork.FeedbackAnswerRepository.AddAsync(answer);
            }

            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<IEnumerable<FeedbackResponseDto>> GetAllFeedbacksAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            DateTime? utcStart = startDate.HasValue ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc) : null;
            DateTime? utcEnd = endDate.HasValue ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc) : null;

            var list = await _unitOfWork.FeedbackResponseRepository.GetAsync(
                filter: r => (!utcStart.HasValue || r.SubmittedAt >= utcStart.Value) &&
                             (!utcEnd.HasValue || r.SubmittedAt <= utcEnd.Value),
                orderBy: q => q.OrderByDescending(r => r.SubmittedAt),
                includeProperties: "Answers.Question"
            );

            return list.Select(r => MapToResponseDto(r));
        }

        public async Task<FeedbackResponseDto?> GetFeedbackByIdAsync(Guid id)
        {
            var list = await _unitOfWork.FeedbackResponseRepository.GetAsync(
                filter: r => r.Id == id,
                includeProperties: "Answers.Question"
            );
            var item = list.FirstOrDefault();
            return item == null ? null : MapToResponseDto(item);
        }

        public async Task<byte[]> ExportFeedbacksToExcelAsync(DateTime startDate, DateTime endDate)
        {
            // Normalize dates to start of day and end of day
            var normalizedStart = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            var normalizedEnd = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            // Fetch responses in range
            var responses = (await _unitOfWork.FeedbackResponseRepository.GetAsync(
                filter: r => r.SubmittedAt >= normalizedStart && r.SubmittedAt <= normalizedEnd,
                orderBy: q => q.OrderBy(r => r.SubmittedAt),
                includeProperties: "Answers.Question"
            )).ToList();

            // Set license context for EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Phản hồi ý kiến");

                // Title banner
                worksheet.Cells["A1:D1"].Merge = true;
                worksheet.Cells["A1"].Value = "BÁO CÁO PHẢN HỒI NGƯỜI DÙNG";
                worksheet.Cells["A1"].Style.Font.Size = 16;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                worksheet.Cells["A2:D2"].Merge = true;
                worksheet.Cells["A2"].Value = $"Thời gian lọc: từ {normalizedStart:dd/MM/yyyy} đến {normalizedEnd:dd/MM/yyyy}";
                worksheet.Cells["A2"].Style.Font.Italic = true;

                // Get all questions that have been answered in this batch to build columns dynamically
                var uniqueQuestions = responses
                    .SelectMany(r => r.Answers)
                    .Select(a => a.Question)
                    .Where(q => q != null)
                    .GroupBy(q => q!.Id)
                    .Select(g => g.First())
                    .OrderBy(q => q!.Order)
                    .ToList();

                // Setup header columns
                int currentCol = 1;
                worksheet.Cells[4, currentCol++].Value = "STT";
                worksheet.Cells[4, currentCol++].Value = "Họ Tên";
                worksheet.Cells[4, currentCol++].Value = "Email";
                worksheet.Cells[4, currentCol++].Value = "Ngày gửi";

                // Add dynamic question columns
                int questionStartCol = currentCol;
                foreach (var q in uniqueQuestions)
                {
                    worksheet.Cells[4, currentCol++].Value = q!.QuestionText;
                }

                int totalCols = currentCol - 1;

                // Format Header row
                var headerRange = worksheet.Cells[4, 1, 4, totalCols];
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
                headerRange.Style.Font.Color.SetColor(Color.White);
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Row(4).Height = 28;

                // Populate Rows
                int rowIdx = 5;
                int stt = 1;
                foreach (var response in responses)
                {
                    worksheet.Cells[rowIdx, 1].Value = stt++;
                    worksheet.Cells[rowIdx, 2].Value = response.UserFullName ?? "Ẩn danh";
                    worksheet.Cells[rowIdx, 3].Value = response.UserEmail ?? "N/A";
                    worksheet.Cells[rowIdx, 4].Value = response.SubmittedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

                    // For each question column, find user's answer
                    for (int i = 0; i < uniqueQuestions.Count; i++)
                    {
                        var qId = uniqueQuestions[i]!.Id;
                        var answer = response.Answers.FirstOrDefault(a => a.QuestionId == qId);
                        worksheet.Cells[rowIdx, questionStartCol + i].Value = answer?.AnswerText ?? "";
                    }

                    // Format zebra striping
                    if (rowIdx % 2 == 0)
                    {
                        var rowRange = worksheet.Cells[rowIdx, 1, rowIdx, totalCols];
                        rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        rowRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 245, 249));
                    }

                    rowIdx++;
                }

                // Add borders to the table
                if (responses.Any())
                {
                    var tableRange = worksheet.Cells[4, 1, rowIdx - 1, totalCols];
                    tableRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    tableRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    tableRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    tableRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }

                // Auto-fit columns
                for (int col = 1; col <= totalCols; col++)
                {
                    worksheet.Column(col).AutoFit();
                    // Set maximum width to prevent extremely wide columns for text answers
                    if (worksheet.Column(col).Width > 50)
                    {
                        worksheet.Column(col).Width = 50;
                        worksheet.Column(col).Style.WrapText = true;
                    }
                }

                return package.GetAsByteArray();
            }
        }

        // ==========================================
        // HELPERS
        // ==========================================

        private FeedbackQuestionDto MapToQuestionDto(FeedbackQuestion q)
        {
            return new FeedbackQuestionDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Options = q.Options,
                Order = q.Order,
                IsActive = q.IsActive,
                CreatedAt = q.CreatedAt
            };
        }

        private FeedbackResponseDto MapToResponseDto(FeedbackResponse r)
        {
            return new FeedbackResponseDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserEmail = r.UserEmail,
                UserFullName = r.UserFullName,
                SubmittedAt = r.SubmittedAt,
                Answers = (r.Answers ?? new List<FeedbackAnswer>()).Select(a => new FeedbackAnswerDto
                {
                    QuestionId = a.QuestionId,
                    QuestionText = a.Question?.QuestionText ?? "N/A",
                    QuestionType = a.Question?.QuestionType ?? "Text",
                    AnswerText = a.AnswerText
                }).ToList()
            };
        }
    }
}
