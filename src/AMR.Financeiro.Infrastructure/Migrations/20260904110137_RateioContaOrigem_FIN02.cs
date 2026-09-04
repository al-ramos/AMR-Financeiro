using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMR.Financeiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RateioContaOrigem_FIN02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // FIN-02 — a regra de rateio referenciava a conta de origem apenas por uma
            // descricao livre. Sem conseguir encontrar a conta, o servico distribuia
            // R$ 1.000 fixos e persistia o resultado como apuracao.
            migrationBuilder.AddColumn<int>(
                name: "ContaOrigemId",
                table: "regrasrateio",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            // Preenche o vinculo nas regras existentes casando a descricao livre com a
            // descricao da conta na mesma filial. O que nao casar fica em 0, e a FK
            // abaixo rejeita a migracao — e melhor parar aqui do que deixar a regra
            // apontando para lugar nenhum e voltar a ratear um numero que ninguem apurou.
            migrationBuilder.Sql(@"
                UPDATE regrasrateio
                   SET ContaOrigemId = COALESCE((
                       SELECT p.Id
                         FROM planodecontas p
                        WHERE p.CdFilial = regrasrateio.CdFilial
                          AND p.AceitaLancamentos = 1
                          AND (p.Descricao = regrasrateio.ContaOrigemDescricao
                            OR (p.Codigo || ' - ' || p.Descricao) = regrasrateio.ContaOrigemDescricao)
                        LIMIT 1
                   ), 0);
            ");

            migrationBuilder.CreateIndex(
                name: "IX_regrasrateio_ContaOrigemId",
                table: "regrasrateio",
                column: "ContaOrigemId");

            migrationBuilder.AddForeignKey(
                name: "FK_regrasrateio_planodecontas_ContaOrigemId",
                table: "regrasrateio",
                column: "ContaOrigemId",
                principalTable: "planodecontas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        // O Down remove a coluna. A descricao livre continua na tabela, entao a regra
        // volta ao estado anterior — sem saber de que conta o valor sai.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_regrasrateio_planodecontas_ContaOrigemId",
                table: "regrasrateio");

            migrationBuilder.DropIndex(
                name: "IX_regrasrateio_ContaOrigemId",
                table: "regrasrateio");

            migrationBuilder.DropColumn(
                name: "ContaOrigemId",
                table: "regrasrateio");
        }
    }
}
