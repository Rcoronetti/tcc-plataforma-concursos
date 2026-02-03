
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TccConcursos.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToDisciplinaNome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_disciplinas_ConcursoId",
                table: "disciplinas");

            migrationBuilder.CreateIndex(
                name: "IX_disciplinas_ConcursoId_Nome",
                table: "disciplinas",
                columns: new[] { "ConcursoId", "Nome" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_disciplinas_ConcursoId_Nome",
                table: "disciplinas");

            migrationBuilder.CreateIndex(
                name: "IX_disciplinas_ConcursoId",
                table: "disciplinas",
                column: "ConcursoId");
        }
    }
}
