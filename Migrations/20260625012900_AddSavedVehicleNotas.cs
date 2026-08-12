using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerificarApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSavedVehicleNotas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "NotaConfiabilidade",
                table: "SavedVehicles",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NotaConforto",
                table: "SavedVehicles",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NotaConsumo",
                table: "SavedVehicles",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NotaCustoManutencao",
                table: "SavedVehicles",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NotaDesempenho",
                table: "SavedVehicles",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NotaEspacoInterno",
                table: "SavedVehicles",
                type: "decimal(65,30)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotaConfiabilidade",
                table: "SavedVehicles");

            migrationBuilder.DropColumn(
                name: "NotaConforto",
                table: "SavedVehicles");

            migrationBuilder.DropColumn(
                name: "NotaConsumo",
                table: "SavedVehicles");

            migrationBuilder.DropColumn(
                name: "NotaCustoManutencao",
                table: "SavedVehicles");

            migrationBuilder.DropColumn(
                name: "NotaDesempenho",
                table: "SavedVehicles");

            migrationBuilder.DropColumn(
                name: "NotaEspacoInterno",
                table: "SavedVehicles");
        }
    }
}
