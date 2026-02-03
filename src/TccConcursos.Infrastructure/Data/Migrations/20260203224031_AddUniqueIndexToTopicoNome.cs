using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TccConcursos.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToTopicoNome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_topicos_DisciplinaId",
                table: "topicos");

            migrationBuilder.CreateIndex(
                name: "IX_topicos_DisciplinaId_Nome",
                table: "topicos",
                columns: new[] { "DisciplinaId", "Nome" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_topicos_DisciplinaId_Nome",
                table: "topicos");

            migrationBuilder.CreateIndex(
                name: "IX_topicos_DisciplinaId",
                table: "topicos",
                column: "DisciplinaId");
        }
    }
}
