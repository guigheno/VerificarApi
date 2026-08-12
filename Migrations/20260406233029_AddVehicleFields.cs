using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerificarApi.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Combustivel",
                table: "Vehicles",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Descricao",
                table: "Vehicles",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "FipeData",
                table: "Vehicles",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "FipeValor",
                table: "Vehicles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NotaConfiabilidade",
                table: "Vehicles",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NotaConforto",
                table: "Vehicles",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NotaConsumo",
                table: "Vehicles",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NotaCustoManutencao",
                table: "Vehicles",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NotaDesempenho",
                table: "Vehicles",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NotaEspacoInterno",
                table: "Vehicles",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PontosNegativos",
                table: "Vehicles",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PontosPositivos",
                table: "Vehicles",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProblemasCronicos",
                table: "Vehicles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Combustivel",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Descricao",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "FipeData",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "FipeValor",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "NotaConfiabilidade",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "NotaConforto",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "NotaConsumo",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "NotaCustoManutencao",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "NotaDesempenho",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "NotaEspacoInterno",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "PontosNegativos",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "PontosPositivos",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ProblemasCronicos",
                table: "Vehicles");
        }
    }
}
