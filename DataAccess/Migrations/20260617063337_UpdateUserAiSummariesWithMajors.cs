using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserAiSummariesWithMajors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Top3UniversityIds",
                table: "UserAiSummaries",
                newName: "Top3Recommendations");

            migrationBuilder.RenameColumn(
                name: "Next5UniversityIds",
                table: "UserAiSummaries",
                newName: "Next5Recommendations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Top3Recommendations",
                table: "UserAiSummaries",
                newName: "Top3UniversityIds");

            migrationBuilder.RenameColumn(
                name: "Next5Recommendations",
                table: "UserAiSummaries",
                newName: "Next5UniversityIds");
        }
    }
}
