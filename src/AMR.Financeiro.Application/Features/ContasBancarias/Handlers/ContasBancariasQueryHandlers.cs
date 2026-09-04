using MediatR;
using AMR.Financeiro.Application.Features.ContasBancarias.Dtos;
using AMR.Financeiro.Application.Features.ContasBancarias.Queries;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.ContasBancarias.Handlers;

public class GetContasBancariasHandler(IContaBancariaRepository repo)
    : IRequestHandler<GetContasBancariasQuery, List<ContaBancariaDto>>
{
    public async Task<List<ContaBancariaDto>> Handle(GetContasBancariasQuery req, CancellationToken ct)
    {
        var contas = await repo.ListarAsync(req.IncluirInativas, ct);
        var saldos = await repo.ObterSaldosAsync(req.IncluirInativas, ct);

        return contas.Select(c => new ContaBancariaDto(
            c.Id, c.Nome, c.Banco, c.Agencia, c.Conta, c.TipoConta, c.Ativa,
            c.SaldoInicial, c.DataSaldoInicial,
            saldos.TryGetValue(c.Id, out var s) ? s : c.SaldoInicial
        )).ToList();
    }
}

public class GetContaBancariaByIdHandler(IContaBancariaRepository repo)
    : IRequestHandler<GetContaBancariaByIdQuery, ContaBancariaDto?>
{
    public async Task<ContaBancariaDto?> Handle(GetContaBancariaByIdQuery req, CancellationToken ct)
    {
        var c = await repo.ObterPorIdAsync(req.Id, ct);
        if (c is null) return null;
        var saldo = await repo.ObterSaldoAsync(c.Id, ct) ?? c.SaldoInicial;
        return new ContaBancariaDto(
            c.Id, c.Nome, c.Banco, c.Agencia, c.Conta, c.TipoConta, c.Ativa,
            c.SaldoInicial, c.DataSaldoInicial, saldo);
    }
}

public class GetExtratoHandler(IContaBancariaRepository repo)
    : IRequestHandler<GetExtratoQuery, List<ExtratoItemDto>>
{
    public async Task<List<ExtratoItemDto>> Handle(GetExtratoQuery req, CancellationToken ct)
    {
        var lancamentos = await repo.ObterExtratoAsync(req.ContaId, ct);
        return lancamentos.Select(l => new ExtratoItemDto(
            l.Id,
            l.DataLancamento,
            l.Historico,
            l.Tipo.ToString(),
            l.Valor,
            l.Conta?.Descricao ?? string.Empty
        )).ToList();
    }
}
