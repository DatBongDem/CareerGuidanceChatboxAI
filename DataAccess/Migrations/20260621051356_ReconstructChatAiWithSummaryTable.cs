using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ReconstructChatAiWithSummaryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasEnoughInfo",
                table: "ChatAiSessions");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "ChatAiSessions");

            migrationBuilder.DropColumn(
                name: "Next5Recommendations",
                table: "ChatAiSessions");

            migrationBuilder.DropColumn(
                name: "SummaryText",
                table: "ChatAiSessions");

            migrationBuilder.DropColumn(
                name: "Top3Recommendations",
                table: "ChatAiSessions");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ChatAiSessions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ChatAiSummaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SummaryText = table.Column<string>(type: "text", nullable: false),
                    Recommendations = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatAiSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatAiSummaries_ChatAiSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "ChatAiSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatAiSummaries_SessionId",
                table: "ChatAiSummaries",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatAiSummaries");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ChatAiSessions");

            migrationBuilder.AddColumn<bool>(
                name: "HasEnoughInfo",
                table: "ChatAiSessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "ChatAiSessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Next5Recommendations",
                table: "ChatAiSessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SummaryText",
                table: "ChatAiSessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Top3Recommendations",
                table: "ChatAiSessions",
                type: "text",
                nullable: true);
        }
    }
}
