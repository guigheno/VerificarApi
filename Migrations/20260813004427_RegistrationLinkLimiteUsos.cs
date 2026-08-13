using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerificarApi.Migrations
{
    /// <inheritdoc />
    public partial class RegistrationLinkLimiteUsos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegistrationLinkToken",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "LimiteUsos",
                table: "RegistrationLinks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsosAtuais",
                table: "RegistrationLinks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationLinkToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LimiteUsos",
                table: "RegistrationLinks");

            migrationBuilder.DropColumn(
                name: "UsosAtuais",
                table: "RegistrationLinks");
        }
    }
}
