using Microsoft.EntityFrameworkCore;
using AMR.Financeiro.Domain.Entities;

namespace AMR.Financeiro.Infrastructure.Data;

public class FinanceiroDbContext(DbContextOptions<FinanceiroDbContext> options) : DbContext(options)
{
    public DbSet<ContaPagar> ContasPagar => Set<ContaPagar>();
    public DbSet<ContaReceber> ContasReceber => Set<ContaReceber>();
    public DbSet<PlanoContas> PlanoContas => Set<PlanoContas>();
    public DbSet<LancamentoFinanceiro> Lancamentos => Set<LancamentoFinanceiro>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<NotaFiscal> NotasFiscais => Set<NotaFiscal>();
    public DbSet<Boleto> Boletos => Set<Boleto>();
    public DbSet<RemessaBancaria> RemessasBancarias => Set<RemessaBancaria>();
    public DbSet<RetornoBancario> RetornosBancarios => Set<RetornoBancario>();

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
    }
}
