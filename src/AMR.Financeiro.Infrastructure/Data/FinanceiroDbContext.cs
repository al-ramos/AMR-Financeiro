using Microsoft.EntityFrameworkCore;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Infrastructure.Repositories;

namespace AMR.Financeiro.Infrastructure.Data;

public class FinanceiroDbContext(DbContextOptions<FinanceiroDbContext> options) : DbContext(options)
{
    public DbSet<ContaPagar> ContasPagar => Set<ContaPagar>();
    public DbSet<ContaReceber> ContasReceber => Set<ContaReceber>();
    public DbSet<PlanoContas> PlanoContas => Set<PlanoContas>();
    public DbSet<PlanoDeContas> PlanoDeContas => Set<PlanoDeContas>();
    public DbSet<LancamentoFinanceiro> Lancamentos => Set<LancamentoFinanceiro>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<NotaFiscal> NotasFiscais => Set<NotaFiscal>();
    public DbSet<Boleto> Boletos => Set<Boleto>();
    public DbSet<RemessaBancaria> RemessasBancarias => Set<RemessaBancaria>();
    public DbSet<RetornoBancario> RetornosBancarios => Set<RetornoBancario>();
    public DbSet<ExtratoBancario> ExtratosBancarios => Set<ExtratoBancario>();
    public DbSet<MovimentacaoBancaria> MovimentacoesBancarias => Set<MovimentacaoBancaria>();
    public DbSet<CentroCusto> CentrosCusto => Set<CentroCusto>();
    public DbSet<OrcamentoCC> OrcamentosCC => Set<OrcamentoCC>();
    public DbSet<RegraRateio> RegrasRateio => Set<RegraRateio>();
    public DbSet<RegraRateioDestino> RegraRateioDestinos => Set<RegraRateioDestino>();
    public DbSet<RateioRealizado> RateiosRealizados => Set<RateioRealizado>();
    public DbSet<ContaBancaria> ContasBancarias => Set<ContaBancaria>();
    public DbSet<Parcelamento> Parcelamentos => Set<Parcelamento>();
    public DbSet<Parcela> Parcelas => Set<Parcela>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ContaPagar
        mb.Entity<ContaPagar>(e =>
        {
            e.ToTable("ContasPagar");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Descricao).HasMaxLength(200).IsRequired();
            e.Property(x => x.Valor).HasPrecision(18, 2);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        });

        // ContaReceber
        mb.Entity<ContaReceber>(e =>
        {
            e.ToTable("ContasReceber");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Descricao).HasMaxLength(200).IsRequired();
            e.Property(x => x.Valor).HasPrecision(18, 2);
            e.Property(x => x.ValorRecebido).HasPrecision(18, 2);
            e.Property(x => x.DocumentoOrigem).HasMaxLength(100);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        });

        // PlanoContas
        mb.Entity<PlanoContas>(e =>
        {
            e.ToTable("PlanoContas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
            e.Property(x => x.Descricao).HasMaxLength(200).IsRequired();
            e.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => new { x.CdFilial, x.Codigo }).IsUnique();
            e.HasOne(x => x.Pai)
             .WithMany(x => x.Filhos)
             .HasForeignKey(x => x.PaiId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // PlanoDeContas — plano gerencial hierárquico (5 níveis) com classificação DRE (Card 23.4)
        mb.Entity<PlanoDeContas>(e =>
        {
            e.ToTable("planodecontas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
            e.Property(x => x.Descricao).HasMaxLength(200).IsRequired();
            e.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Natureza).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.GrupoDRE).HasConversion<string>().HasMaxLength(30);
            e.HasOne<PlanoDeContas>()
             .WithMany()
             .HasForeignKey(x => x.PaiId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.CdFilial, x.Codigo }).IsUnique();
        });

        // LancamentoFinanceiro
        mb.Entity<LancamentoFinanceiro>(e =>
        {
            e.ToTable("Lancamentos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Historico).HasMaxLength(500).IsRequired();
            e.Property(x => x.Valor).HasPrecision(18, 2);
            e.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Origem).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.PlanoContas)
             .WithMany(x => x.Lancamentos)
             .HasForeignKey(x => x.PlanoContasId)
             .OnDelete(DeleteBehavior.Restrict);
            // Card 23.6 — vínculo opcional com conta bancária (saldo/extrato multi-banco)
            e.HasOne<ContaBancaria>()
             .WithMany()
             .HasForeignKey(x => x.ContaBancariaId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.ContaBancariaId);
            // Card 23.5 — vínculo opcional com centro de custo (realizado por CC)
            e.HasOne<CentroCusto>()
             .WithMany()
             .HasForeignKey(x => x.CentroCustoId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.CentroCustoId);
        });

        // Usuario
        mb.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Username).HasMaxLength(100).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(x => x.Role).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Username).IsUnique();
        });

        // NotaFiscal (NF-e)
        mb.Entity<NotaFiscal>(e =>
        {
            e.ToTable("notasfiscais");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Modelo).HasConversion<int>();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Ambiente).HasConversion<int>();
            e.Property(x => x.ChaveAcesso).HasMaxLength(44);
            e.Property(x => x.ProtocoloAutorizacao).HasMaxLength(30);
            e.Property(x => x.MotivoRejeicao).HasMaxLength(500);
            e.Property(x => x.JustificativaCancelamento).HasMaxLength(255);
            e.Property(x => x.ValorTotal).HasPrecision(18, 2);
            e.Property(x => x.NomeDestinatario).HasMaxLength(200).IsRequired();
            e.Property(x => x.CpfCnpjDestinatario).HasMaxLength(14).IsRequired();
            e.HasIndex(x => x.ChaveAcesso).IsUnique();
            e.HasIndex(x => new { x.CdFilial, x.Modelo, x.Serie, x.NumeroNF }).IsUnique();
        });

        // Boleto
        mb.Entity<Boleto>(e =>
        {
            e.ToTable("boletos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Banco).HasConversion<int>();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.NossoNumero).HasMaxLength(20).IsRequired();
            e.Property(x => x.LinhaDigitavel).HasMaxLength(60).IsRequired();
            e.Property(x => x.CodigoBarras).HasMaxLength(44).IsRequired();
            e.Property(x => x.Valor).HasPrecision(18, 2);
            e.Property(x => x.ValorPago).HasPrecision(18, 2);
            e.Property(x => x.SacadoNome).HasMaxLength(200).IsRequired();
            e.Property(x => x.SacadoCpfCnpj).HasMaxLength(14);
            e.Property(x => x.SacadoEndereco).HasMaxLength(300);
            e.Property(x => x.Instrucao1).HasMaxLength(200);
            e.Property(x => x.Instrucao2).HasMaxLength(200);
            e.HasIndex(x => new { x.Banco, x.NossoNumero }).IsUnique();
        });

        // RemessaBancaria
        mb.Entity<RemessaBancaria>(e =>
        {
            e.ToTable("remessasbancarias");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Banco).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.TipoCnab).HasConversion<string>().HasMaxLength(10);
            e.Property(x => x.NomeArquivo).HasMaxLength(100).IsRequired();
            e.Property(x => x.ValorTotal).HasPrecision(18, 2);
        });

        // RetornoBancario
        mb.Entity<RetornoBancario>(e =>
        {
            e.ToTable("retornosbancarios");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Banco).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.ArquivoNome).HasMaxLength(100).IsRequired();
            e.Property(x => x.ValorLiquidado).HasPrecision(18, 2);
        });

        // ExtratoBancario (Conciliação)
        mb.Entity<ExtratoBancario>(e =>
        {
            e.ToTable("extratosbancarios");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Banco).HasMaxLength(100).IsRequired();
            e.Property(x => x.ContaCorrente).HasMaxLength(30).IsRequired();
            e.Property(x => x.SaldoInicial).HasPrecision(18, 2);
            e.Property(x => x.SaldoFinal).HasPrecision(18, 2);
            e.Property(x => x.TotalCreditos).HasPrecision(18, 2);
            e.Property(x => x.TotalDebitos).HasPrecision(18, 2);
            e.Property(x => x.Formato).HasConversion<string>().HasMaxLength(10);
            e.Property(x => x.ArquivoOriginal).HasMaxLength(200);
            e.HasIndex(x => x.CdFilial);
        });

        // MovimentacaoBancaria (Conciliação)
        mb.Entity<MovimentacaoBancaria>(e =>
        {
            e.ToTable("movimentacoesbancarias");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Valor).HasPrecision(18, 2);
            e.Property(x => x.Descricao).HasMaxLength(300).IsRequired();
            e.Property(x => x.CodigoDoc).HasMaxLength(50);
            e.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(10);
            e.Property(x => x.StatusConciliacao).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.ConciliadoPor).HasMaxLength(200);
            e.HasOne<ExtratoBancario>()
             .WithMany()
             .HasForeignKey(x => x.ExtratoId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.ExtratoId, x.StatusConciliacao });
        });

        // ExtratoHash (idempotência da importação)
        mb.Entity<ExtratoHash>(e =>
        {
            e.ToTable("extratos_hashes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Hash).HasMaxLength(64).IsRequired();
            e.HasIndex(x => new { x.CdFilial, x.Hash }).IsUnique();
        });

        // CentroCusto (Card 23.5)
        mb.Entity<CentroCusto>(e =>
        {
            e.ToTable("centroscusto");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
            e.Property(x => x.Descricao).HasMaxLength(200).IsRequired();
            e.Property(x => x.ResponsavelNome).HasMaxLength(200).IsRequired();
            e.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(20);
            e.HasOne<CentroCusto>()
             .WithMany()
             .HasForeignKey(x => x.PaiId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.CdFilial, x.Codigo }).IsUnique();
        });

        // OrcamentoCC (Card 23.5)
        mb.Entity<OrcamentoCC>(e =>
        {
            e.ToTable("orcamentoscc");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.ContaDescricao).HasMaxLength(200).IsRequired();
            e.Property(x => x.ValorOrcado).HasPrecision(18, 2);
            e.Property(x => x.ValorRealizado).HasPrecision(18, 2);
            e.HasOne<CentroCusto>()
             .WithMany()
             .HasForeignKey(x => x.CentroCustoId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.CentroCustoId, x.ContaDescricao, x.Ano, x.Mes }).IsUnique();
        });

        // RegraRateio (Card 23.5)
        mb.Entity<RegraRateio>(e =>
        {
            e.ToTable("regrasrateio");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Nome).HasMaxLength(150).IsRequired();
            e.Property(x => x.ContaOrigemDescricao).HasMaxLength(200).IsRequired();
            e.Property(x => x.TipoBase).HasConversion<string>().HasMaxLength(20);
            e.HasMany(x => x.Destinos)
             .WithOne()
             .HasForeignKey(x => x.RegraRateioId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // RegraRateioDestino (Card 23.5)
        mb.Entity<RegraRateioDestino>(e =>
        {
            e.ToTable("regrasrateiodestinos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Percentual).HasPrecision(5, 2);
            e.Property(x => x.ValorBase).HasPrecision(18, 2);
            e.HasOne<CentroCusto>()
             .WithMany()
             .HasForeignKey(x => x.CentroCustoId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // RateioRealizado (Card 23.5)
        mb.Entity<RateioRealizado>(e =>
        {
            e.ToTable("rateiosrealizados");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.ValorRateado).HasPrecision(18, 2);
            e.Property(x => x.PercentualAplicado).HasPrecision(5, 2);
            e.HasOne<RegraRateio>()
             .WithMany()
             .HasForeignKey(x => x.RegraRateioId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.RegraRateioId, x.Competencia });
        });

        // ContaBancaria (Card 23.6 — Multi-banco)
        mb.Entity<ContaBancaria>(e =>
        {
            e.ToTable("contas_bancarias");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Nome).HasMaxLength(100).IsRequired();
            e.Property(x => x.Banco).HasMaxLength(100);
            e.Property(x => x.Agencia).HasMaxLength(20);
            e.Property(x => x.Conta).HasMaxLength(30);
            e.Property(x => x.TipoConta).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Ativa).HasDefaultValue(true);
            e.Property(x => x.SaldoInicial).HasPrecision(18, 2);
            e.HasIndex(x => x.Ativa);
        });

        // Parcelamento (Card 23.6)
        mb.Entity<Parcelamento>(e =>
        {
            e.ToTable("parcelamentos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Descricao).HasMaxLength(200).IsRequired();
            e.Property(x => x.ValorTotal).HasPrecision(18, 2);
            e.Property(x => x.TipoVinculo).HasConversion<string>().HasMaxLength(20);
            e.HasMany(x => x.Parcelas)
             .WithOne()
             .HasForeignKey(x => x.ParcelamentoId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Parcela (Card 23.6)
        mb.Entity<Parcela>(e =>
        {
            e.ToTable("parcelas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.ValorParcela).HasPrecision(18, 2);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne<ContaBancaria>()
             .WithMany()
             .HasForeignKey(x => x.ContaBancariaId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.Status, x.DataVencimento });
        });
    }
}
