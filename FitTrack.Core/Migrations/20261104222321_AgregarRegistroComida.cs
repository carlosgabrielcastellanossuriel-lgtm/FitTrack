using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitTrack.Core.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRegistroComida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistroComida",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResumenDiarioId = table.Column<int>(type: "int", nullable: false),
                    AlimentoId = table.Column<int>(type: "int", nullable: false),
                    Gramos = table.Column<double>(type: "float", nullable: false),
                    Calorias = table.Column<double>(type: "float", nullable: false),
                    Proteinas = table.Column<double>(type: "float", nullable: false),
                    Carbohidratos = table.Column<double>(type: "float", nullable: false),
                    Grasas = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistroComida", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistroComida_Alimento_AlimentoId",
                        column: x => x.AlimentoId,
                        principalTable: "Alimento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistroComida_ResumenDiario_ResumenDiarioId",
                        column: x => x.ResumenDiarioId,
                        principalTable: "ResumenDiario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistroComida_AlimentoId",
                table: "RegistroComida",
                column: "AlimentoId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistroComida_ResumenDiarioId",
                table: "RegistroComida",
                column: "ResumenDiarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistroComida");
        }
    }
}
