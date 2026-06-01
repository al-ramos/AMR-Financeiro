using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMR.Financeiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SqliteInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""ContasPagar"" (
                    ""Id""            INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""CdFilial""      INTEGER NOT NULL,
                    ""Descricao""     TEXT    NOT NULL,
                    ""Valor""         REAL    NOT NULL,
                    ""Vencimento""    TEXT    NOT NULL,
                    ""DataPagamento"" TEXT,
                    ""Status""        TEXT    NOT NULL,
                    ""CriadoEm""      TEXT    NOT NULL
                );

                CREATE TABLE IF NOT EXISTS ""ContasReceber"" (
                    ""Id""               INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""CdFilial""         INTEGER NOT NULL,
                    ""Descricao""        TEXT    NOT NULL,
                    ""Valor""            REAL    NOT NULL,
                    ""Vencimento""       TEXT    NOT NULL,
                    ""DataRecebimento""  TEXT,
                    ""ValorRecebido""    REAL,
                    ""Status""           TEXT    NOT NULL,
                    ""CriadoEm""         TEXT    NOT NULL,
                    ""DocumentoOrigem""  TEXT
                );

                CREATE TABLE IF NOT EXISTS ""PlanoContas"" (
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

                CREATE TABLE IF NOT EXISTS ""Lancamentos"" (
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

                CREATE TABLE IF NOT EXISTS ""Usuarios"" (
                    ""Id""           INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""Username""     TEXT NOT NULL,
                    ""PasswordHash"" TEXT NOT NULL,
                    ""Role""         TEXT NOT NULL,
                    ""CriadoEm""     TEXT NOT NULL
                );

                CREATE INDEX IF NOT EXISTS ""IX_Lancamentos_PlanoContasId""
                    ON ""Lancamentos"" (""PlanoContasId"");

                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_PlanoContas_CdFilial_Codigo""
                    ON ""PlanoContas"" (""CdFilial"", ""Codigo"");

                CREATE INDEX IF NOT EXISTS ""IX_PlanoContas_PaiId""
                    ON ""PlanoContas"" (""PaiId"");

                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Usuarios_Username""
                    ON ""Usuarios"" (""Username"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Lancamentos");
            migrationBuilder.DropTable(name: "PlanoContas");
            migrationBuilder.DropTable(name: "ContasPagar");
            migrationBuilder.DropTable(name: "ContasReceber");
            migrationBuilder.DropTable(name: "Usuarios");
        }
    }
}
