using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentFieldsToEduRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountName",
                table: "EduRegistrations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "EduRegistrations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bin",
                table: "EduRegistrations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckoutUrl",
                table: "EduRegistrations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentDescription",
                table: "EduRegistrations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountName",
                table: "EduRegistrations");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "EduRegistrations");

            migrationBuilder.DropColumn(
                name: "Bin",
                table: "EduRegistrations");

            migrationBuilder.DropColumn(
                name: "CheckoutUrl",
                table: "EduRegistrations");

            migrationBuilder.DropColumn(
                name: "PaymentDescription",
                table: "EduRegistrations");
        }
    }
}
