using MediatR;
using AMR.Financeiro.Application.Interfaces;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.Boletos.Commands;

public record GerarRemessaCommand(
    int CdFilial,
    BancoBoleto Banco,
    TipoCnab TipoCnab,
    List<int> BoletoIds) : IRequest<GerarRemessaResult>;

public record GerarRemessaResult(
    int RemessaId,
    string NomeArquivo,
    string CnabBase64,
    int TotalBoletos,
    decimal ValorTotal);

public class GerarRemessaCommandHandler(
    IBoletoRepository boletoRepository,
    IBoletoService boletoService)
    : IRequestHandler<GerarRemessaCommand, GerarRemessaResult>
{
    public async Task<GerarRemessaResult> Handle(GerarRemessaCommand cmd, CancellationToken ct)
    {
        var boletos = new List<Boleto>();
        foreach (var id in cmd.BoletoIds)
        {
            var boleto = await boletoRepository.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Boleto {id} não encontrado.");
            boletos.Add(boleto);
        }

        var remessaGerada = await boletoService.GerarRemessaAsync(cmd.Banco, cmd.TipoCnab, boletos, ct);

        var remessa = new RemessaBancaria(
            cmd.CdFilial,
            cmd.Banco,
            cmd.TipoCnab,
            remessaGerada.CnabConteudo,
            remessaGerada.NomeArquivo,
            remessaGerada.TotalBoletos,
            remessaGerada.ValorTotal);

        await boletoRepository.AddRemessaAsync(remessa, ct);

        // Boletos incluídos na remessa passam a "Registrado".
        foreach (var boleto in boletos.Where(b => b.Status == StatusBoleto.Gerado))
        {
            boleto.MarcarRegistrado();
            await boletoRepository.UpdateAsync(boleto, ct);
        }

        return new GerarRemessaResult(
            remessa.Id,
            remessaGerada.NomeArquivo,
            remessaGerada.CnabBase64,
            remessaGerada.TotalBoletos,
            remessaGerada.ValorTotal);
    }
}
