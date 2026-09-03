using MediatR;
using AMR.Financeiro.Application.Interfaces;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.Boletos.Commands;

public class GerarBoletoCommandHandler(
    IContaReceberRepository contaReceberRepository,
    IBoletoRepository boletoRepository,
    IBoletoService boletoService)
    : IRequestHandler<GerarBoletoCommand, GerarBoletoResult>
{
    public async Task<GerarBoletoResult> Handle(GerarBoletoCommand cmd, CancellationToken ct)
    {
        var conta = await contaReceberRepository.ObterPorIdAsync(cmd.ContaReceberId, ct)
            ?? throw new KeyNotFoundException($"Conta a receber {cmd.ContaReceberId} não encontrada.");

        var proximoNossoNumero = await boletoRepository.GetNextNossoNumeroAsync(cmd.Banco, cmd.CdFilial, ct);

        // ContaReceber ainda não carrega dados cadastrais do sacado —
        // usa a descrição da conta como identificação até o cadastro de clientes ser integrado.
        var sacadoNome = conta.Descricao;
        var sacadoCpfCnpj = string.Empty;
        var sacadoEndereco = string.Empty;

        var gerado = await boletoService.GerarAsync(
            cmd.Banco, proximoNossoNumero,
            sacadoNome, sacadoCpfCnpj, sacadoEndereco,
            conta.Valor, cmd.Vencimento,
            cmd.Instrucao1, cmd.Instrucao2, ct);

        var boleto = new Boleto(
            cmd.CdFilial,
            cmd.ContaReceberId,
            cmd.Banco,
            gerado.NossoNumero,
            gerado.LinhaDigitavel,
            gerado.CodigoBarras,
            conta.Valor,
            cmd.Vencimento,
            sacadoNome,
            sacadoCpfCnpj,
            sacadoEndereco,
            cmd.Instrucao1,
            cmd.Instrucao2,
            gerado.PdfBase64);

        await boletoRepository.AddAsync(boleto, ct);

        return new GerarBoletoResult(boleto.Id, boleto.LinhaDigitavel, boleto.CodigoBarras, boleto.PdfBase64);
    }
}
