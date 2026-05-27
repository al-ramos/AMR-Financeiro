using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMR.Financeiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQLite: usar SQL direto com INTEGER PRIMARY KEY para auto-increment correto.
            // DROP IF EXISTS para idempotência caso o banco no EFS esteja em estado parcial.
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS ""Usuarios"";

                CREATE TABLE ""Usuarios"" (
                    ""Id""           INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""Username""     TEXT NOT NULL,
                    ""PasswordHash"" TEXT NOT NULL,
                    ""Role""         TEXT NOT NULL,
                    ""CriadoEm""     TEXT NOT NULL
                );

                CREATE UNIQUE INDEX ""IX_Usuarios_Username"" ON ""Usuarios"" (""Username"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Usuarios");
        }
    }
}

