using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMR.Financeiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnificaPlanoDeContas_FIN01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // FIN-01 — o plano de contas era duas tabelas. Lancamentos.PlanoContasId
            // apontava para o plano legado (PlanoContas), enquanto a tela e a DRE ja
            // usavam planodecontas. Os Id colidiam sem significar a mesma conta, e o
            // lancamento era contabilizado em outra conta sem erro.
            //
            // Antes de repontar a FK, os lancamentos existentes sao remapeados pelo par
            // (CdFilial, Codigo) — o mesmo vinculo que a entidade ja documentava. Um
            // lancamento cuja conta legada nao exista no plano unico fica com
            // PlanoContasId = NULL e a FK rejeita a migracao, em vez de deixar o valor
            // antigo apontando silenciosamente para outra conta.
            migrationBuilder.Sql(@"
                UPDATE Lancamentos
                   SET PlanoContasId = (
                       SELECT n.Id
                         FROM planodecontas n
                         JOIN PlanoContas a ON a.Codigo = n.Codigo
                                           AND a.CdFilial = n.CdFilial
                        WHERE a.Id = Lancamentos.PlanoContasId
                   );
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_Lancamentos_PlanoContas_PlanoContasId",
                table: "Lancamentos");

            migrationBuilder.DropTable(
                name: "PlanoContas");

            migrationBuilder.AddForeignKey(
                name: "FK_Lancamentos_planodecontas_PlanoContasId",
                table: "Lancamentos",
                column: "PlanoContasId",
                principalTable: "planodecontas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        // O Down recria a tabela legada vazia. Os lancamentos remapeados no Up nao
        // voltam a apontar para os Id antigos — eles nao existem mais. Reverter esta
        // migracao com dados exige restaurar backup, nao rodar o Down.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lancamentos_planodecontas_PlanoContasId",
                table: "Lancamentos");

            migrationBuilder.CreateTable(
                name: "PlanoContas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PaiId = table.Column<int>(type: "INTEGER", nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CdFilial = table.Column<int>(type: "INTEGER", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_PlanoContas_CdFilial_Codigo",
                table: "PlanoContas",
                columns: new[] { "CdFilial", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanoContas_PaiId",
                table: "PlanoContas",
                column: "PaiId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lancamentos_PlanoContas_PlanoContasId",
                table: "Lancamentos",
                column: "PlanoContasId",
                principalTable: "PlanoContas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
