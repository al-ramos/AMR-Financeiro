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

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ContaPagar
        mb.Entity<ContaPagar>(e =>
        {
            e.ToTable("ContasPagar");
            e.HasKey(x => x.Id);
            e.Property(x => x.Descricao).HasMaxLength(200).IsRequired();
            e.Property(x => x.Valor).HasPrecision(18, 2);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        });

        // ContaReceber
        mb.Entity<ContaReceber>(e =>
        {
            e.ToTable("ContasReceber");
            e.HasKey(x => x.Id);
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
            e.Property(x => x.Username).HasMaxLength(100).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(x => x.Role).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Username).IsUnique();
        });
    }
}
