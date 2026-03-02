using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TccConcursos.Infrastructure.Data.Migrations
{
    public partial class AddUsuariosTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Senha = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Endereco = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Bio = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CriadoEmUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEmUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Cpf",
                table: "usuarios",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Email",
                table: "usuarios",
                column: "Email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
