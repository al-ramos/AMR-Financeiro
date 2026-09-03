using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMR.Financeiro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiBancoParcelamentoAging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Valor",
                table: "Lancamentos",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "REAL",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<int>(
                name: "CentroCustoId",
                table: "Lancamentos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContaBancariaId",
                table: "Lancamentos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ValorRecebido",
                table: "ContasReceber",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "REAL",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Valor",
                table: "ContasReceber",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "REAL",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Valor",
                table: "ContasPagar",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "REAL",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.CreateTable(
                name: "boletos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CdFilial = table.Column<int>(type: "INTEGER", nullable: false),
                    ContaReceberId = table.Column<int>(type: "INTEGER", nullable: false),
                    Banco = table.Column<int>(type: "INTEGER", nullable: false),
                    NossoNumero = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    LinhaDigitavel = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    CodigoBarras = table.Column<string>(type: "TEXT", maxLength: 44, nullable: false),
                    Valor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Vencimento = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DataEmissao = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    SacadoNome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SacadoCpfCnpj = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    SacadoEndereco = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Instrucao1 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Instrucao2 = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DataPagamento = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    ValorPago = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    PdfBase64 = table.Column<string>(type: "TEXT", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_boletos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "centroscusto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CdFilial = table.Column<int>(type: "INTEGER", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PaiId = table.Column<int>(type: "INTEGER", nullable: true),
                    Nivel = table.Column<int>(type: "INTEGER", nullable: false),
                    ResponsavelNome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_centroscusto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_centroscusto_centroscusto_PaiId",
                        column: x => x.PaiId,
                        principalTable: "centroscusto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "contas_bancarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Banco = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Agencia = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Conta = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    TipoConta = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Ativa = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    SaldoInicial = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DataSaldoInicial = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contas_bancarias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "extratos_hashes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CdFilial = table.Column<int>(type: "INTEGER", nullable: false),
                    Hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ExtratoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ImportadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extratos_hashes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "extratosbancarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CdFilial = table.Column<int>(type: "INTEGER", nullable: false),
                    Banco = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ContaCorrente = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    DataInicio = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DataFim = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    SaldoInicial = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    SaldoFinal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalCreditos = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalDebitos = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Formato = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    ArquivoOriginal = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ImportadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extratosbancarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notasfiscais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CdFilial = table.Column<int>(type: "INTEGER", nullable: false),
                    Modelo = table.Column<int>(type: "INTEGER", nullable: false),
                    Serie = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroNF = table.Column<long>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Ambiente = table.Column<int>(type: "INTEGER", nullable: false),
                    ChaveAcesso = table.Column<string>(type: "TEXT", maxLength: 44, nullable: true),
                    ProtocoloAutorizacao = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    XmlAssinado = table.Column<string>(type: "TEXT", nullable: true),
                    XmlAutorizado = table.Column<string>(type: "TEXT", nullable: true),
                    DataAutorizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataCancelamento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MotivoRejeicao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    JustificativaCancelamento = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ValorTotal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    NomeDestinatario = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CpfCnpjDestinatario = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notasfiscais", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "parcelamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ValorTotal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    NumeroParcelas = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoVinculo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    VinculoId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parcelamentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "planodecontas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CdFilial = table.Column<int>(type: "INTEGER", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Natureza = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Nivel = table.Column<int>(type: "INTEGER", nullable: false),
                    PaiId = table.Column<int>(type: "INTEGER", nullable: true),
                    AceitaLancamentos = table.Column<bool>(type: "INTEGER", nullable: false),
                    GrupoDRE = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    OrdemExibicao = table.Column<int>(type: "INTEGER", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planodecontas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_planodecontas_planodecontas_PaiId",
                        column: x => x.PaiId,
                        principalTable: "planodecontas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "regrasrateio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CdFilial = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    ContaOrigemDescricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TipoBase = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regrasrateio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "remessasbancarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CdFilial = table.Column<int>(type: "INTEGER", nullable: false),
                    Banco = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TipoCnab = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    ArquivoCnab = table.Column<string>(type: "TEXT", nullable: false),
                    NomeArquivo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TotalBoletos = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_remessasbancarias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "retornosbancarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CdFilial = table.Column<int>(type: "INTEGER", nullable: false),
                    Banco = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ArquivoNome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ArquivoConteudo = table.Column<string>(type: "TEXT", nullable: false),
                    TotalRegistros = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalLiquidados = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorLiquidado = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ProcessadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_retornosbancarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "orcamentoscc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CentroCustoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContaDescricao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Ano = table.Column<int>(type: "INTEGER", nullable: false),
                    Mes = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorOrcado = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ValorRealizado = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orcamentoscc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_orcamentoscc_centroscusto_CentroCustoId",
                        column: x => x.CentroCustoId,
                        principalTable: "centroscusto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "movimentacoesbancarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExtratoId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataLancamento = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Valor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    CodigoDoc = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    StatusConciliacao = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    LancamentoId = table.Column<int>(type: "INTEGER", nullable: true),
                    ConciliadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ConciliadoPor = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimentacoesbancarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_movimentacoesbancarias_extratosbancarios_ExtratoId",
                        column: x => x.ExtratoId,
                        principalTable: "extratosbancarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parcelas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParcelamentoId = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroParcela = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorParcela = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ContaBancariaId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parcelas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_parcelas_contas_bancarias_ContaBancariaId",
                        column: x => x.ContaBancariaId,
                        principalTable: "contas_bancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_parcelas_parcelamentos_ParcelamentoId",
                        column: x => x.ParcelamentoId,
                        principalTable: "parcelamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rateiosrealizados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RegraRateioId = table.Column<int>(type: "INTEGER", nullable: false),
                    CentroCustoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorRateado = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    PercentualAplicado = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    Competencia = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rateiosrealizados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rateiosrealizados_regrasrateio_RegraRateioId",
                        column: x => x.RegraRateioId,
                        principalTable: "regrasrateio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "regrasrateiodestinos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RegraRateioId = table.Column<int>(type: "INTEGER", nullable: false),
                    CentroCustoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Percentual = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    ValorBase = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regrasrateiodestinos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_regrasrateiodestinos_centroscusto_CentroCustoId",
                        column: x => x.CentroCustoId,
                        principalTable: "centroscusto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_regrasrateiodestinos_regrasrateio_RegraRateioId",
                        column: x => x.RegraRateioId,
                        principalTable: "regrasrateio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_CentroCustoId",
                table: "Lancamentos",
                column: "CentroCustoId");

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_ContaBancariaId",
                table: "Lancamentos",
                column: "ContaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_boletos_Banco_NossoNumero",
                table: "boletos",
                columns: new[] { "Banco", "NossoNumero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_centroscusto_CdFilial_Codigo",
                table: "centroscusto",
                columns: new[] { "CdFilial", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_centroscusto_PaiId",
                table: "centroscusto",
                column: "PaiId");

            migrationBuilder.CreateIndex(
                name: "IX_contas_bancarias_Ativa",
                table: "contas_bancarias",
                column: "Ativa");

            migrationBuilder.CreateIndex(
                name: "IX_extratos_hashes_CdFilial_Hash",
                table: "extratos_hashes",
                columns: new[] { "CdFilial", "Hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_extratosbancarios_CdFilial",
                table: "extratosbancarios",
                column: "CdFilial");

            migrationBuilder.CreateIndex(
                name: "IX_movimentacoesbancarias_ExtratoId_StatusConciliacao",
                table: "movimentacoesbancarias",
                columns: new[] { "ExtratoId", "StatusConciliacao" });

            migrationBuilder.CreateIndex(
                name: "IX_notasfiscais_CdFilial_Modelo_Serie_NumeroNF",
                table: "notasfiscais",
                columns: new[] { "CdFilial", "Modelo", "Serie", "NumeroNF" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notasfiscais_ChaveAcesso",
                table: "notasfiscais",
                column: "ChaveAcesso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_orcamentoscc_CentroCustoId_ContaDescricao_Ano_Mes",
                table: "orcamentoscc",
                columns: new[] { "CentroCustoId", "ContaDescricao", "Ano", "Mes" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parcelas_ContaBancariaId",
                table: "parcelas",
                column: "ContaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_parcelas_ParcelamentoId",
                table: "parcelas",
                column: "ParcelamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_parcelas_Status_DataVencimento",
                table: "parcelas",
                columns: new[] { "Status", "DataVencimento" });

            migrationBuilder.CreateIndex(
                name: "IX_planodecontas_CdFilial_Codigo",
                table: "planodecontas",
                columns: new[] { "CdFilial", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_planodecontas_PaiId",
                table: "planodecontas",
                column: "PaiId");

            migrationBuilder.CreateIndex(
                name: "IX_rateiosrealizados_RegraRateioId_Competencia",
                table: "rateiosrealizados",
                columns: new[] { "RegraRateioId", "Competencia" });

            migrationBuilder.CreateIndex(
                name: "IX_regrasrateiodestinos_CentroCustoId",
                table: "regrasrateiodestinos",
                column: "CentroCustoId");

            migrationBuilder.CreateIndex(
                name: "IX_regrasrateiodestinos_RegraRateioId",
                table: "regrasrateiodestinos",
                column: "RegraRateioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lancamentos_centroscusto_CentroCustoId",
                table: "Lancamentos",
                column: "CentroCustoId",
                principalTable: "centroscusto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lancamentos_contas_bancarias_ContaBancariaId",
                table: "Lancamentos",
                column: "ContaBancariaId",
                principalTable: "contas_bancarias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lancamentos_centroscusto_CentroCustoId",
                table: "Lancamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Lancamentos_contas_bancarias_ContaBancariaId",
                table: "Lancamentos");

            migrationBuilder.DropTable(
                name: "boletos");

            migrationBuilder.DropTable(
                name: "extratos_hashes");

            migrationBuilder.DropTable(
                name: "movimentacoesbancarias");

            migrationBuilder.DropTable(
                name: "notasfiscais");

            migrationBuilder.DropTable(
                name: "orcamentoscc");

            migrationBuilder.DropTable(
                name: "parcelas");

            migrationBuilder.DropTable(
                name: "planodecontas");

            migrationBuilder.DropTable(
                name: "rateiosrealizados");

            migrationBuilder.DropTable(
                name: "regrasrateiodestinos");

            migrationBuilder.DropTable(
                name: "remessasbancarias");

            migrationBuilder.DropTable(
                name: "retornosbancarios");

            migrationBuilder.DropTable(
                name: "extratosbancarios");

            migrationBuilder.DropTable(
                name: "contas_bancarias");

            migrationBuilder.DropTable(
                name: "parcelamentos");

            migrationBuilder.DropTable(
                name: "centroscusto");

            migrationBuilder.DropTable(
                name: "regrasrateio");

            migrationBuilder.DropIndex(
                name: "IX_Lancamentos_CentroCustoId",
                table: "Lancamentos");

            migrationBuilder.DropIndex(
                name: "IX_Lancamentos_ContaBancariaId",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "CentroCustoId",
                table: "Lancamentos");

            migrationBuilder.DropColumn(
                name: "ContaBancariaId",
                table: "Lancamentos");

            migrationBuilder.AlterColumn<decimal>(
                name: "Valor",
                table: "Lancamentos",
                type: "REAL",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "ValorRecebido",
                table: "ContasReceber",
                type: "REAL",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Valor",
                table: "ContasReceber",
                type: "REAL",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Valor",
                table: "ContasPagar",
                type: "REAL",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldPrecision: 18,
                oldScale: 2);
        }
    }
}
