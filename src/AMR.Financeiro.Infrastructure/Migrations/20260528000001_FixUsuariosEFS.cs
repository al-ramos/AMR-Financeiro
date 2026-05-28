using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMR.Financeiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUsuariosEFS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tabela Usuarios no EFS pode ter estrutura errada (PK table-level sem AUTOINCREMENT)
            // pois a migration AddUsuarios foi registrada em __EFMigrationsHistory antes do fix.
            // Esta migration recria com a estrutura correta — DROP IF EXISTS garante idempotência.
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
