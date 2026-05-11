using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerificarApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCiclo2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VehicleAnalyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Marca = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Modelo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AnoModelo = table.Column<int>(type: "int", nullable: false),
                    ProblemasCronicos = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PontosPositivos = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PontosNegativos = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResumoGeral = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NotaConfiabilidade = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    NotaCustoManutencao = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    NotaConforto = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    NotaDesempenho = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    NotaConsumo = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    NotaEspacoInterno = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    GeradoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ModeloIA = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleAnalyses", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VehicleHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Marca = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Modelo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AnoModelo = table.Column<int>(type: "int", nullable: false),
                    Placa = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipoEvento = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataEvento = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Fonte = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleHistories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleAnalyses");

            migrationBuilder.DropTable(
                name: "VehicleHistories");
        }
    }
}
