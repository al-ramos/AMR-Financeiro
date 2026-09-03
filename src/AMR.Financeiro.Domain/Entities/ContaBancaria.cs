using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.Domain.Entities;

/// <summary>
/// Conta bancária da empresa (Card 23.6 — Multi-banco).
/// O saldo atual nunca é persistido: é sempre calculado a partir do
/// SaldoInicial + lançamentos vinculados desde DataSaldoInicial.
/// </summary>
public class ContaBancaria
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Banco { get; private set; } = string.Empty;
    public string Agencia { get; private set; } = string.Empty;
    public string Conta { get; private set; } = string.Empty;
    public TipoContaBancaria TipoConta { get; private set; }
    public bool Ativa { get; private set; } = true;
    public decimal SaldoInicial { get; private set; }
    public DateTime DataSaldoInicial { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    protected ContaBancaria() { }

    public ContaBancaria(
        string nome,
        string banco,
        string agencia,
        string conta,
        TipoContaBancaria tipoConta,
        decimal saldoInicial,
        DateTime dataSaldoInicial)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da conta bancária é obrigatório.", nameof(nome));

        Nome = nome.Trim();
        Banco = banco?.Trim() ?? string.Empty;
        Agencia = agencia?.Trim() ?? string.Empty;
        Conta = conta?.Trim() ?? string.Empty;
        TipoConta = tipoConta;
        SaldoInicial = saldoInicial;
        DataSaldoInicial = dataSaldoInicial;
    }

    public void Atualizar(
        string nome,
        string banco,
        string agencia,
        string conta,
        TipoContaBancaria tipoConta,
        decimal saldoInicial,
        DateTime dataSaldoInicial)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da conta bancária é obrigatório.", nameof(nome));

        Nome = nome.Trim();
        Banco = banco?.Trim() ?? string.Empty;
        Agencia = agencia?.Trim() ?? string.Empty;
        Conta = conta?.Trim() ?? string.Empty;
        TipoConta = tipoConta;
        SaldoInicial = saldoInicial;
        DataSaldoInicial = dataSaldoInicial;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Soft delete — a conta sai das listagens mas preserva o histórico.</summary>
    public void Desativar()
    {
        Ativa = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reativar()
    {
        Ativa = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
