using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancieroPersonal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PersonaPorDefectoCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioId",
                table: "Categorias",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Categorias");
        }
    }
}
