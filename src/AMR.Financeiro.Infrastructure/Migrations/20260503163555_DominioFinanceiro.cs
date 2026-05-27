using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMR.Financeiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DominioFinanceiro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQLite não suporta ALTER COLUMN diretamente; EF Core gera uma reconstrução
            // de tabela via ef_temp_, mas falha com "AUTOINCREMENT is only allowed on an
            // INTEGER PRIMARY KEY" por usar "int" em vez de "INTEGER" no DDL gerado.
            // Como SQLite não impõe maxLength em TEXT, a mudança é no-op — substituído
            // por SQL direto com a reconstrução correta.
            migrationBuilder.Sql(@"
                CREATE TABLE ""ef_temp_ContasPagar"" (
                    ""Id""            INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    ""CdFilial""      INTEGER NOT NULL,
                    ""Descricao""     TEXT    NOT NULL,
                    ""Valor""         REAL    NOT NULL,
                    ""Vencimento""    TEXT    NOT NULL,
                    ""DataPagamento"" TEXT,
                    ""Status""        TEXT    NOT NULL,
                    ""CriadoEm""      TEXT    NOT NULL
                );
                INSERT INTO ""ef_temp_ContasPagar"" SELECT * FROM ""ContasPagar"";
                DROP TABLE ""ContasPagar"";
                ALTER TABLE ""ef_temp_ContasPagar"" RENAME TO ""ContasPagar"";
            ");

            // SQLite: PRIMARY KEY como constraint de tabela NÃO cria alias do rowid —
            // o Id não é gerado automaticamente. Usar SQL direto com coluna-level PRIMARY KEY.
            // DROP IF EXISTS garante idempotência caso o banco no EFS tenha tabelas em estado
            // inconsistente de execuções anteriores que falharam parcialmente.
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS ""Lancamentos"";
                DROP TABLE IF EXISTS ""PlanoContas"";
                DROP TABLE IF EXISTS ""ContasReceber"";

                CREATE TABLE ""ContasReceber"" (
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
                    ""Id""               INTEGER PRIMARY KEY AUTOINCREMENT,
                    ""CdFilial""         INTEGER NOT NULL,
                    ""PlanoContasId""    INTEGER NOT NULL,
                    ""Tipo""             TEXT    NOT NULL,
                    ""Origem""           TEXT    NOT NULL,
                    ""Valor""            REAL    NOT NULL,
                    ""DataLancamento""   TEXT    NOT NULL,
                    ""Historico""        TEXT    NOT NULL,
                    ""DocumentoOrigemId"" INTEGER,
                    ""CriadoEm""         TEXT    NOT NULL,
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
            migrationBuilder.DropTable(
                name: "ContasReceber");

            migrationBuilder.DropTable(
                name: "Lancamentos");

            migrationBuilder.DropTable(
                name: "PlanoContas");

            // Reverso do no-op acima — SQLite TEXT não tem maxLength, sem reconstrução necessária.
            migrationBuilder.Sql("SELECT 1;");
        }
    }
}

