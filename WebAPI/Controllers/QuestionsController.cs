using BusinessLogic.DTOs.ChatAI.Question;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using DataAccess.DataContext;
using DataAccess.Entities.ChatAI;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionService _service;
        private readonly ApplicationDbContext _context;

        public QuestionsController(IQuestionService service, ApplicationDbContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create(CreateQuestionDto createDto)
        {
            try
            {
                var result = await _service.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(Guid id, UpdateQuestionDto updateDto)
        {
            try
            {
                var result = await _service.UpdateAsync(id, updateDto);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategoryId(Guid categoryId)
        {
            var result = await _service.GetByCategoryIdAsync(categoryId);
            return Ok(result);
        }

        private const string TemplateContent = """
I. Bộ câu hỏi
1. Câu hỏi?
A. Đáp án 1
B. Đáp án 2
C. Đáp án 3
D. Đáp án 4
E. Đáp án 5
F. Đáp án 6
G. Khác (vui lòng nhập)

II. Bộ câu hỏi
1. Câu hỏi?
A. Sửa chữa, lắp ráp hoặc làm việc với máy móc
B. Nghiên cứu, tìm hiểu nguyên nhân của một vấn đề
C. Vẽ, thiết kế hoặc sáng tạo nội dung
D. Hỗ trợ, hướng dẫn hoặc giúp đỡ người khác
E. Thuyết phục, kinh doanh hoặc lãnh đạo
F. Sắp xếp, quản lý hồ sơ và dữ liệu
G. Khác (vui lòng nhập)
""";

        [HttpGet("template")]
        [AllowAnonymous] // Allow anyone to download the template
        public IActionResult GetTemplate()
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(TemplateContent);
            return File(bytes, "application/msword", "mau_cau_hoi.doc");
        }

        [HttpPost("import")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ImportQuestions(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                string textContent;
                using (var stream = file.OpenReadStream())
                {
                    textContent = ExtractTextFromStream(stream, file.FileName);
                }

                if (string.IsNullOrEmpty(textContent))
                {
                    return BadRequest("Failed to read text from file or file is empty.");
                }

                var lines = textContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                // Clear existing questions, options, and categories
                var oldCategories = _context.QuestionCategories.ToList();
                _context.QuestionCategories.RemoveRange(oldCategories);
                await _context.SaveChangesAsync();

                QuestionCategory? currentCategory = null;
                Question? currentQuestion = null;
                int categoryOrder = 0;
                int questionOrder = 0;
                int optionOrder = 0;

                var categoriesToInsert = new List<QuestionCategory>();

                foreach (var rawLine in lines)
                {
                    var line = rawLine.Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    // Ignore lines that are goals/measurements
                    if (line.StartsWith("Mục đích:", StringComparison.OrdinalIgnoreCase) || 
                        line.StartsWith("Đo nhóm:", StringComparison.OrdinalIgnoreCase) ||
                        Regex.IsMatch(line, @"^[A-Z]\s*→"))
                    {
                        continue;
                    }

                    // Check Category header
                    var matchCategory = Regex.Match(line, @"^(I|II|III|IV|V|VI)\.\s*([^0-9\n\r]+)(.*)$");
                    if (matchCategory.Success)
                    {
                        categoryOrder++;
                        var catName = matchCategory.Groups[2].Value.Trim();
                        var remaining = matchCategory.Groups[3].Value.Trim();

                        currentCategory = new QuestionCategory
                        {
                            Id = Guid.NewGuid(),
                            Name = catName,
                            DisplayOrder = categoryOrder,
                            Questions = new List<Question>()
                        };
                        categoriesToInsert.Add(currentCategory);
                        questionOrder = 0;

                        if (!string.IsNullOrEmpty(remaining))
                        {
                            var matchMergedQ = Regex.Match(remaining, @"^(\d+)\.\s*(.*)$");
                            if (matchMergedQ.Success)
                            {
                                questionOrder++;
                                var qContent = matchMergedQ.Groups[2].Value.Trim();
                                currentQuestion = new Question
                                {
                                    Id = Guid.NewGuid(),
                                    CategoryId = currentCategory.Id,
                                    Content = qContent,
                                    DisplayOrder = questionOrder,
                                    AllowCustomAnswer = false,
                                    IsActice = "Yes",
                                    Options = new List<QuestionOption>()
                                };
                                currentCategory.Questions.Add(currentQuestion);
                                optionOrder = 0;
                            }
                        }
                        continue;
                    }

                    // Check regular question line
                    var matchQuestion = Regex.Match(line, @"^(\d+)\.\s*(.*)$");
                    if (matchQuestion.Success)
                    {
                        questionOrder++;
                        var qContent = matchQuestion.Groups[2].Value.Trim();
                        currentQuestion = new Question
                        {
                            Id = Guid.NewGuid(),
                            CategoryId = currentCategory != null ? currentCategory.Id : Guid.Empty,
                            Content = qContent,
                            DisplayOrder = questionOrder,
                            AllowCustomAnswer = false,
                            IsActice = "Yes",
                            Options = new List<QuestionOption>()
                        };
                        if (currentCategory != null)
                        {
                            currentCategory.Questions.Add(currentQuestion);
                        }
                        optionOrder = 0;
                        continue;
                    }

                    // Check option line
                    var matchOption = Regex.Match(line, @"^([A-H])\.\s*(.*)$");
                    if (matchOption.Success && currentQuestion != null)
                    {
                        optionOrder++;
                        var optCode = matchOption.Groups[1].Value.Trim();
                        var optContent = matchOption.Groups[2].Value.Trim();

                        if (optContent.Contains("Khác") || optContent.Contains("vui lòng nhập"))
                        {
                            currentQuestion.AllowCustomAnswer = true;
                        }

                        var option = new QuestionOption
                        {
                            Id = Guid.NewGuid(),
                            QuestionId = currentQuestion.Id,
                            OptionCode = optCode,
                            Content = optContent,
                            DisplayOrder = optionOrder
                        };
                        currentQuestion.Options.Add(option);
                        continue;
                    }
                }

                _context.QuestionCategories.AddRange(categoriesToInsert);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Imported successfully!", categoryCount = categoriesToInsert.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        private string ExtractTextFromStream(Stream stream, string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            if (extension == ".docx")
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    var entry = archive.GetEntry("word/document.xml");
                    if (entry == null) throw new Exception("Invalid docx file structure.");

                    using (var entryStream = entry.Open())
                    {
                        var xmlDoc = new XmlDocument();
                        xmlDoc.Load(entryStream);

                        var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                        nsmgr.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");

                        var paragraphs = xmlDoc.SelectNodes("//w:p", nsmgr);
                        var sb = new System.Text.StringBuilder();
                        if (paragraphs != null)
                        {
                            foreach (XmlNode p in paragraphs)
                            {
                                var textNodes = p.SelectNodes(".//w:t", nsmgr);
                                var pText = "";
                                if (textNodes != null)
                                {
                                    foreach (XmlNode t in textNodes)
                                    {
                                        pText += t.InnerText;
                                    }
                                }
                                if (!string.IsNullOrWhiteSpace(pText))
                                {
                                    sb.AppendLine(pText.Trim());
                                }
                            }
                        }
                        return sb.ToString();
                    }
                }
            }
            else
            {
                using (var reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
