using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TccConcursos.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateInitialDomainTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "concursos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DataProva = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_concursos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "disciplinas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConcursoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_disciplinas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_disciplinas_concursos_ConcursoId",
                        column: x => x.ConcursoId,
                        principalTable: "concursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "topicos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisciplinaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_topicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_topicos_disciplinas_DisciplinaId",
                        column: x => x.DisciplinaId,
                        principalTable: "disciplinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sessoes_estudo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TopicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    QuestoesTotal = table.Column<int>(type: "integer", nullable: true),
                    QuestoesAcertos = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessoes_estudo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sessoes_estudo_topicos_TopicoId",
                        column: x => x.TopicoId,
                        principalTable: "topicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_disciplinas_ConcursoId",
                table: "disciplinas",
                column: "ConcursoId");

            migrationBuilder.CreateIndex(
                name: "IX_sessoes_estudo_TopicoId_Inicio",
                table: "sessoes_estudo",
                columns: new[] { "TopicoId", "Inicio" });

            migrationBuilder.CreateIndex(
                name: "IX_topicos_DisciplinaId",
                table: "topicos",
                column: "DisciplinaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sessoes_estudo");

            migrationBuilder.DropTable(
                name: "topicos");

            migrationBuilder.DropTable(
                name: "disciplinas");

            migrationBuilder.DropTable(
                name: "concursos");
        }
    }
}
