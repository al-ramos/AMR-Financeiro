using MediatR;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.Boletos.Commands;

public record CancelarBoletoCommand(int BoletoId) : IRequest<CancelarBoletoResult>;
public record CancelarBoletoResult(bool Sucesso, string? MensagemErro);

public class CancelarBoletoCommandHandler(IBoletoRepository boletoRepository)
    : IRequestHandler<CancelarBoletoCommand, CancelarBoletoResult>
{
    public async Task<CancelarBoletoResult> Handle(CancelarBoletoCommand cmd, CancellationToken ct)
    {
        var boleto = await boletoRepository.GetByIdAsync(cmd.BoletoId, ct)
            ?? throw new KeyNotFoundException($"Boleto {cmd.BoletoId} não encontrado.");

        if (boleto.Status != StatusBoleto.Gerado)
            return new CancelarBoletoResult(false, $"Apenas boleto com status Gerado pode ser cancelado. Status atual: {boleto.Status}.");

        boleto.Cancelar();
        await boletoRepository.UpdateAsync(boleto, ct);

        return new CancelarBoletoResult(true, null);
    }
}
