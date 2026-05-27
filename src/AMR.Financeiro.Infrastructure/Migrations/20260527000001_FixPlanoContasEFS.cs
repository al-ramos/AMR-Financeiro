using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMR.Financeiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPlanoContasEFS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // O banco no EFS pode ter PlanoContas e Lancamentos com estrutura errada
            // (PRIMARY KEY como table-level constraint, sem AUTOINCREMENT) — consequência
            // de uma migration anterior que correu parcialmente antes do fix.
            // Esta migration força o recreate com a estrutura correta.
            // DROP + CREATE é idempotente: se já estiver correto, recria do zero (seed repopula).
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS ""Lancamentos"";
                DROP TABLE IF EXISTS ""PlanoContas"";

                CREATE TABLE ""PlanoContas"" (
                    ""Id""        INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""CdFilial""  INTEGER NOT NULL,
                    ""Codigo""    TEXT    NOT NULL,
                    ""Descricao"" TEXT    NOT NULL,
                    ""Tipo""      TEXT    NOT NULL,
                    ""PaiId""     INTEGER,
                    ""Ativo""     INTEGER NOT NULL,
                    ""CriadoEm""  TEXT    NOT NULL,
                    CONSTRAINT ""FK_PlanoContas_PlanoContas_PaiId""
                        FOREIGN KEY (""PaiId"") REFERENCES ""PlanoContas"" (""Id"") ON DELETE RESTRICT
                );

                CREATE TABLE ""Lancamentos"" (
                    ""Id""                INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""CdFilial""          INTEGER NOT NULL,
                    ""PlanoContasId""     INTEGER NOT NULL,
                    ""Tipo""              TEXT    NOT NULL,
                    ""Origem""            TEXT    NOT NULL,
                    ""Valor""             REAL    NOT NULL,
                    ""DataLancamento""    TEXT    NOT NULL,
                    ""Historico""         TEXT    NOT NULL,
                    ""DocumentoOrigemId"" INTEGER,
                    ""CriadoEm""          TEXT    NOT NULL,
                    CONSTRAINT ""FK_Lancamentos_PlanoContas_PlanoContasId""
                        FOREIGN KEY (""PlanoContasId"") REFERENCES ""PlanoContas"" (""Id"") ON DELETE RESTRICT
                );

                CREATE INDEX ""IX_Lancamentos_PlanoContasId""
                    ON ""Lancamentos"" (""PlanoContasId"");

                CREATE UNIQUE INDEX ""IX_PlanoContas_CdFilial_Codigo""
                    ON ""PlanoContas"" (""CdFilial"", ""Codigo"");

                CREATE INDEX ""IX_PlanoContas_PaiId""
                    ON ""PlanoContas"" (""PaiId"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Lancamentos");
            migrationBuilder.DropTable(name: "PlanoContas");
        }
    }
}
