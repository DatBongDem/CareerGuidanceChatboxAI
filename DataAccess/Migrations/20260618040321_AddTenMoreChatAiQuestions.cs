using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTenMoreChatAiQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "AllowCustomAnswer", "CategoryId", "Content", "DisplayOrder", "IsActice" },
                values: new object[,]
                {
                    { new Guid("f0f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"), true, new Guid("b1a2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), "Bạn có xu hướng thích làm việc với dữ liệu/con số (phân tích, lập trình), với con người (tư vấn, giảng dạy, quản lý), hay với các ý tưởng nghệ thuật và thiết kế?", 6, "Yes" },
                    { new Guid("f1f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"), true, new Guid("b1a2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), "Khi gặp một vấn đề khó, bạn thường thích tự tìm tài liệu để nghiên cứu, hay muốn thảo luận cùng người khác để tìm ra giải pháp nhanh nhất?", 7, "Yes" },
                    { new Guid("f2f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"), true, new Guid("b1a2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), "Khả năng thích ứng của bạn trước sự thay đổi công nghệ hoặc môi trường mới như thế nào? Bạn có thích học hỏi các công cụ công nghệ mới liên tục không?", 8, "Yes" },
                    { new Guid("f3f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"), true, new Guid("b1a2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), "Khi lựa chọn nghề nghiệp tương lai, yếu tố nào quan trọng nhất với bạn (mức lương cao, sự ổn định, cơ hội thăng tiến, hay đóng góp giá trị cho cộng đồng)?", 9, "Yes" },
                    { new Guid("f4f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"), true, new Guid("b1a2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), "Bạn có mong muốn làm việc trong một doanh nghiệp toàn cầu (yêu cầu ngoại ngữ cao và đa văn hóa) hay các doanh nghiệp trong nước/khởi nghiệp linh hoạt?", 10, "Yes" },
                    { new Guid("f5f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"), true, new Guid("b1a2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), "Phong cách làm việc ưa thích của bạn là gì (lên kế hoạch chi tiết trước khi làm, hay làm đến đâu linh hoạt giải quyết đến đó)?", 11, "Yes" },
                    { new Guid("f6f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"), true, new Guid("b1a2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), "Hãy chia sẻ về một dự án hoặc thành tích nào đó trong quá khứ khiến bạn cảm thấy tự hào và hứng thú nhất?", 12, "Yes" },
                    { new Guid("f7f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"), true, new Guid("b1a2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), "Mức độ chịu áp lực công việc của bạn ở mức nào (thích công việc nhịp độ nhanh đầy thử thách, hay công việc nhịp độ vừa phải có tính cân bằng cuộc sống)?", 13, "Yes" },
                    { new Guid("f8f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"), true, new Guid("b1a2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), "Bạn tự đánh giá khả năng giao tiếp và truyền đạt ý tưởng của mình bằng lời nói hoặc văn bản ở mức độ nào?", 14, "Yes" },
                    { new Guid("f9f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"), true, new Guid("b1a2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), "Ngoài việc học trên lớp, bạn có thường xuyên tự học các kỹ năng ngoài lề (như thiết kế, lập trình cơ bản, viết lách, hay kỹ năng mềm) không?", 15, "Yes" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "Questions", keyColumn: "Id", keyValue: new Guid("f0f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"));
            migrationBuilder.DeleteData(table: "Questions", keyColumn: "Id", keyValue: new Guid("f1f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"));
            migrationBuilder.DeleteData(table: "Questions", keyColumn: "Id", keyValue: new Guid("f2f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"));
            migrationBuilder.DeleteData(table: "Questions", keyColumn: "Id", keyValue: new Guid("f3f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"));
            migrationBuilder.DeleteData(table: "Questions", keyColumn: "Id", keyValue: new Guid("f4f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"));
            migrationBuilder.DeleteData(table: "Questions", keyColumn: "Id", keyValue: new Guid("f5f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"));
            migrationBuilder.DeleteData(table: "Questions", keyColumn: "Id", keyValue: new Guid("f6f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"));
            migrationBuilder.DeleteData(table: "Questions", keyColumn: "Id", keyValue: new Guid("f7f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"));
            migrationBuilder.DeleteData(table: "Questions", keyColumn: "Id", keyValue: new Guid("f8f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"));
            migrationBuilder.DeleteData(table: "Questions", keyColumn: "Id", keyValue: new Guid("f9f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"));
        }
    }
}
