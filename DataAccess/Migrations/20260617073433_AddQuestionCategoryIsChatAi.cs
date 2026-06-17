using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionCategoryIsChatAi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsChatAi",
                table: "QuestionCategories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "QuestionCategories",
                columns: new[] { "Id", "DisplayOrder", "IsChatAi", "Name" },
                values: new object[] { new Guid("b1a2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), 100, true, "Trò chuyện hướng nghiệp AI" });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "AllowCustomAnswer", "CategoryId", "Content", "DisplayOrder", "IsActice" },
                values: new object[,]
                {
                    { new Guid("a0f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"), true, new Guid("b1a2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), "Bạn hãy chia sẻ về sở thích hoặc thế mạnh nổi bật của mình trong các môn học ở trường (ví dụ: các môn tự nhiên, xã hội, ngoại ngữ, nghệ thuật...)?", 1, "Yes" },
                    { new Guid("b0f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"), true, new Guid("b1a2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), "Sau khi tốt nghiệp, bạn mong muốn được làm việc trong môi trường như thế nào (ví dụ: văn phòng năng động, nghiên cứu độc lập, ngoài trời, kinh doanh tự do, hay môi trường sáng tạo...)?", 2, "Yes" },
                    { new Guid("c0f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"), true, new Guid("b1a2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), "Khi tham gia hoạt động nhóm hoặc dự án, bạn thường cảm thấy mình làm tốt nhất ở vai trò nào (trưởng nhóm điều phối, người lên ý tưởng, người triển khai chi tiết, hay người hỗ trợ)?", 3, "Yes" },
                    { new Guid("d0f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"), true, new Guid("b1a2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), "Bạn có định hướng hay mong muốn đặc biệt nào về vị trí địa lý của trường đại học (ví dụ: học gần nhà, học ở thành phố lớn...) hoặc điều kiện tài chính của gia đình không?", 4, "Yes" },
                    { new Guid("e0f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"), true, new Guid("b1a2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"), "Mục tiêu nghề nghiệp hoặc hình mẫu công việc mơ ước của bạn trong vòng 5 năm tới là gì?", 5, "Yes" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("a0f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"));

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("b0f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"));

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("c0f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"));

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("d0f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"));

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: new Guid("e0f2b3e4-c5d6-47e8-b9a0-c1d2e3f4a5b6"));

            migrationBuilder.DeleteData(
                table: "QuestionCategories",
                keyColumn: "Id",
                keyValue: new Guid("b1a2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d"));

            migrationBuilder.DropColumn(
                name: "IsChatAi",
                table: "QuestionCategories");
        }
    }
}
