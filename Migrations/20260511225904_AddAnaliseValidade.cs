using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerificarApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAnaliseValidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ProximaAtualizacaoEm",
                table: "VehicleAnalyses",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaConsultaEm",
                table: "VehicleAnalyses",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProximaAtualizacaoEm",
                table: "VehicleAnalyses");

            migrationBuilder.DropColumn(
                name: "UltimaConsultaEm",
                table: "VehicleAnalyses");
        }
    }
}
