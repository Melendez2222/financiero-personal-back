using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancieroPersonal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeudasDetalleYBorradoLogico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Periodos_Anio_Mes",
                table: "Periodos");

            migrationBuilder.AddColumn<bool>(
                name: "Eliminado",
                table: "Periodos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EliminadoEn",
                table: "Periodos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Eliminado",
                table: "Movimientos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EliminadoEn",
                table: "Movimientos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsCuota",
                table: "Movimientos",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "Eliminado",
                table: "Categorias",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EliminadoEn",
                table: "Categorias",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoDeuda",
                table: "Categorias",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Periodos_Anio_Mes",
                table: "Periodos",
                columns: new[] { "Anio", "Mes" },
                unique: true,
                filter: "\"Eliminado\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Periodos_Anio_Mes",
                table: "Periodos");

            migrationBuilder.DropColumn(
                name: "Eliminado",
                table: "Periodos");

            migrationBuilder.DropColumn(
                name: "EliminadoEn",
                table: "Periodos");

            migrationBuilder.DropColumn(
                name: "Eliminado",
                table: "Movimientos");

            migrationBuilder.DropColumn(
                name: "EliminadoEn",
                table: "Movimientos");

            migrationBuilder.DropColumn(
                name: "EsCuota",
                table: "Movimientos");

            migrationBuilder.DropColumn(
                name: "Eliminado",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "EliminadoEn",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "TipoDeuda",
                table: "Categorias");

            migrationBuilder.CreateIndex(
                name: "IX_Periodos_Anio_Mes",
                table: "Periodos",
                columns: new[] { "Anio", "Mes" },
                unique: true);
        }
    }
}
