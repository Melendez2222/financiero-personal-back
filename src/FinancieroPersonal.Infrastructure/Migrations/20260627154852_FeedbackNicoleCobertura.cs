using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancieroPersonal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FeedbackNicoleCobertura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cobertura",
                table: "Categorias",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoDeuda",
                table: "Categorias",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Iniciada");

            migrationBuilder.AddColumn<DateOnly>(
                name: "VigenciaDesde",
                table: "Categorias",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "VigenciaHasta",
                table: "Categorias",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cobertura",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "EstadoDeuda",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "VigenciaDesde",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "VigenciaHasta",
                table: "Categorias");
        }
    }
}
