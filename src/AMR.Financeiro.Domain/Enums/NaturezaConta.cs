namespace AMR.Financeiro.Domain.Enums;

/// <summary>
/// Natureza do saldo da conta contábil:
/// Devedora → saldo = débitos - créditos; Credora → saldo = créditos - débitos.
/// </summary>
public enum NaturezaConta
{
    Devedora,
    Credora
}
