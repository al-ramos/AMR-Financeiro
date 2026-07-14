using MediatR;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;
using AMR.Financeiro.Application.Interfaces;

namespace AMR.Financeiro.Application.Features.NotasFiscais.Commands;

public record CancelarNFeCommand(int NotaFiscalId, string Justificativa) : IRequest<CancelarNFeResult>;
public record CancelarNFeResult(bool Sucesso, string? MensagemErro);

public class CancelarNFeCommandHandler(INFeRepository nfeRepository, INFeService nfeService)
    : IRequestHandler<CancelarNFeCommand, CancelarNFeResult>
{
    public async Task<CancelarNFeResult> Handle(CancelarNFeCommand cmd, CancellationToken ct)
    {
        var nfe = await nfeRepository.GetByIdAsync(cmd.NotaFiscalId, ct)
            ?? throw new KeyNotFoundException($"NF-e {cmd.NotaFiscalId} não encontrada.");

        if (nfe.Status != StatusNFe.Autorizada)
            return new CancelarNFeResult(false, "Apenas NF-e autorizada pode ser cancelada.");

        var result = await nfeService.CancelarAsync(nfe.ChaveAcesso!, cmd.Justificativa, nfe.Ambiente, ct);
        if (result.Sucesso)
            nfe.Cancelar(cmd.Justificativa, DateTime.UtcNow);

        await nfeRepository.UpdateAsync(nfe, ct);
        return new CancelarNFeResult(result.Sucesso, result.MensagemErro);
    }
}
