using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitTrack.Core.Migrations
{
    /// <inheritdoc />
    public partial class AgregarResumenDiario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResumenDiario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    CaloriasAcumuladas = table.Column<double>(type: "float", nullable: false),
                    ProteinasAcumuladas = table.Column<double>(type: "float", nullable: false),
                    CarbohidratosAcumulados = table.Column<double>(type: "float", nullable: false),
                    GrasasAcumuladas = table.Column<double>(type: "float", nullable: false),
                    EstadoActual = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResumenDiario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResumenDiario_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResumenDiario_UsuarioId",
                table: "ResumenDiario",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResumenDiario");
        }
    }
}
