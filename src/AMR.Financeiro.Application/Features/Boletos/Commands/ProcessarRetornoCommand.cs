using MediatR;
using AMR.Financeiro.Application.Interfaces;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.Boletos.Commands;

public record ProcessarRetornoCommand(
    int CdFilial,
    BancoBoleto Banco,
    string ArquivoNome,
    string ArquivoConteudo) : IRequest<ProcessarRetornoResult>;

public record ProcessarRetornoResult(
    int RetornoId,
    int TotalRegistros,
    int TotalLiquidados,
    decimal ValorLiquidado,
    List<string> Erros);

public class ProcessarRetornoCommandHandler(
    IBoletoRepository boletoRepository,
    IContaReceberRepository contaReceberRepository,
    IBoletoService boletoService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ProcessarRetornoCommand, ProcessarRetornoResult>
{
    public async Task<ProcessarRetornoResult> Handle(ProcessarRetornoCommand cmd, CancellationToken ct)
    {
        var itens = await boletoService.ProcessarRetornoAsync(cmd.ArquivoConteudo, cmd.Banco, ct);

        var erros = new List<string>();
        var totalLiquidados = 0;
        decimal valorLiquidado = 0m;

        foreach (var item in itens)
        {
            if (!item.Sucesso)
            {
                if (!string.IsNullOrWhiteSpace(item.Erro))
                    erros.Add($"Nosso número {item.NossoNumero}: {item.Erro}");
                continue;
            }

            var boleto = await boletoRepository.GetByNossoNumeroAsync(item.NossoNumero, cmd.Banco, ct);
            if (boleto is null)
            {
                erros.Add($"Nosso número {item.NossoNumero}: boleto não encontrado.");
                continue;
            }

            try
            {
                boleto.MarcarPago(item.DataPagamento, item.ValorPago);
                await boletoRepository.UpdateAsync(boleto, ct);
            }
            catch (InvalidOperationException ex)
            {
                erros.Add($"Nosso número {item.NossoNumero}: {ex.Message}");
                continue;
            }

            var conta = await contaReceberRepository.ObterPorIdAsync(boleto.ContaReceberId, ct);
            if (conta is null)
            {
                erros.Add($"Nosso número {item.NossoNumero}: conta a receber {boleto.ContaReceberId} não encontrada.");
            }
            else
            {
                conta.Receber(item.DataPagamento, item.ValorPago);
                contaReceberRepository.Atualizar(conta);
                await unitOfWork.SaveChangesAsync(ct);
            }

            totalLiquidados++;
            valorLiquidado += item.ValorPago;
        }

        var retorno = new RetornoBancario(
            cmd.CdFilial,
            cmd.Banco,
            cmd.ArquivoNome,
            cmd.ArquivoConteudo,
            itens.Count,
            totalLiquidados,
            valorLiquidado);

        await boletoRepository.AddRetornoAsync(retorno, ct);

        return new ProcessarRetornoResult(retorno.Id, itens.Count, totalLiquidados, valorLiquidado, erros);
    }
}
