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

            migrationBuilder.CreateTable(
                name: "ContasReceber",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CdFilial = table.Column<int>(nullable: false),
                    Descricao = table.Column<string>(maxLength: 200, nullable: false),
                    Valor = table.Column<decimal>(type: "REAL", precision: 18, scale: 2, nullable: false),
                    Vencimento = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DataRecebimento = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    ValorRecebido = table.Column<decimal>(type: "REAL", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<string>(maxLength: 20, nullable: false),
                    CriadoEm = table.Column<DateTime>(nullable: false),
                    DocumentoOrigem = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasReceber", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanoContas",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CdFilial = table.Column<int>(nullable: false),
                    Codigo = table.Column<string>(maxLength: 20, nullable: false),
                    Descricao = table.Column<string>(maxLength: 200, nullable: false),
                    Tipo = table.Column<string>(maxLength: 20, nullable: false),
                    PaiId = table.Column<int>(nullable: true),
                    Ativo = table.Column<bool>(nullable: false),
                    CriadoEm = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanoContas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanoContas_PlanoContas_PaiId",
                        column: x => x.PaiId,
                        principalTable: "PlanoContas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lancamentos",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CdFilial = table.Column<int>(nullable: false),
                    PlanoContasId = table.Column<int>(nullable: false),
                    Tipo = table.Column<string>(maxLength: 20, nullable: false),
                    Origem = table.Column<string>(maxLength: 20, nullable: false),
                    Valor = table.Column<decimal>(type: "REAL", precision: 18, scale: 2, nullable: false),
                    DataLancamento = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Historico = table.Column<string>(maxLength: 500, nullable: false),
                    DocumentoOrigemId = table.Column<int>(nullable: true),
                    CriadoEm = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lancamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lancamentos_PlanoContas_PlanoContasId",
                        column: x => x.PlanoContasId,
                        principalTable: "PlanoContas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_PlanoContasId",
                table: "Lancamentos",
                column: "PlanoContasId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanoContas_CdFilial_Codigo",
                table: "PlanoContas",
                columns: new[] { "CdFilial", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanoContas_PaiId",
                table: "PlanoContas",
                column: "PaiId");
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

