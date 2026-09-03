using MediatR;
using AMR.Financeiro.Application.Features.Parcelamentos.Commands;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.Parcelamentos.Handlers;

public class CriarParcelamentoHandler(IParcelamentoRepository repo, IUnitOfWork uow)
    : IRequestHandler<CriarParcelamentoCommand, int>
{
    public async Task<int> Handle(CriarParcelamentoCommand req, CancellationToken ct)
    {
        var parcelamento = new Parcelamento(
            req.Descricao, req.ValorTotal, req.NumeroParcelas,
            req.TipoVinculo, req.VinculoId);

        parcelamento.GerarParcelas(req.PrimeiroVencimento);

        await repo.AdicionarAsync(parcelamento, ct);
        await uow.SaveChangesAsync(ct);
        return parcelamento.Id;
    }
}

public class PagarParcelaHandler(IParcelamentoRepository repo, IUnitOfWork uow)
    : IRequestHandler<PagarParcelaCommand, bool>
{
    public async Task<bool> Handle(PagarParcelaCommand req, CancellationToken ct)
    {
        var parcelamento = await repo.ObterPorIdAsync(req.ParcelamentoId, ct);
        if (parcelamento is null) return false;

        var parcela = parcelamento.Parcelas.FirstOrDefault(p => p.Id == req.ParcelaId);
        if (parcela is null) return false;

        parcela.MarcarPaga(req.DataPagamento, req.ContaBancariaId);
        await repo.AtualizarAsync(parcelamento, ct);
        await uow.SaveChangesAsync(ct);
        return true;
    }
}
