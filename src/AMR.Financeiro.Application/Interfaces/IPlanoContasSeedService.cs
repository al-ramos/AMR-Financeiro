namespace AMR.Financeiro.Application.Interfaces;

/// <summary>
/// Aplica o plano de contas padrão (CFC) para uma filial.
/// Implementado na Infrastructure (usa o PlanoContasSeeder), exposto aqui para que
/// o handler do comando de seed não dependa da camada de infraestrutura.
/// </summary>
public interface IPlanoContasSeedService
{
    /// <returns>Quantidade de contas criadas (0 se todas já existiam).</returns>
    Task<int> SeedAsync(int cdFilial, CancellationToken ct = default);
}
