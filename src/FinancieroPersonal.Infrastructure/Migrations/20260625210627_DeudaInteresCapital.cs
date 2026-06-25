using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancieroPersonal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeudaInteresCapital : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MontoCapital",
                table: "Movimientos",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CapitalPorCuota",
                table: "Categorias",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MontoCapital",
                table: "Movimientos");

            migrationBuilder.DropColumn(
                name: "CapitalPorCuota",
                table: "Categorias");
        }
    }
}
