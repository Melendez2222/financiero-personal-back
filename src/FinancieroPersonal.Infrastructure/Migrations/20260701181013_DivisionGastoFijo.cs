using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancieroPersonal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DivisionGastoFijo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cobertura",
                table: "Movimientos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoFinDeMes",
                table: "Categorias",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoQuincena",
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
                name: "Cobertura",
                table: "Movimientos");

            migrationBuilder.DropColumn(
                name: "MontoFinDeMes",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "MontoQuincena",
                table: "Categorias");
        }
    }
}
