using MediatR;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;
using AMR.Financeiro.Application.Interfaces;

namespace AMR.Financeiro.Application.Features.NotasFiscais.Commands;

public class EmitirNFeCommandHandler(
    INFeRepository nfeRepository,
    INFeService nfeService) : IRequestHandler<EmitirNFeCommand, EmitirNFeResult>
{
    public async Task<EmitirNFeResult> Handle(EmitirNFeCommand cmd, CancellationToken ct)
    {
        const int serie = 1;
        var numeroNF = await nfeRepository.GetNextNumeroNFAsync(cmd.CdFilial, ModeloNFe.NFe, serie, ct);

        var nfe = new NotaFiscal(
            cmd.CdFilial, ModeloNFe.NFe, serie, numeroNF,
            cmd.Ambiente, cmd.ValorTotal,
            cmd.NomeDestinatario, cmd.CpfCnpjDestinatario);

        await nfeRepository.AddAsync(nfe, ct);

        var request = new NFeEmissaoRequest(
            cmd.CdFilial, cmd.Ambiente,
            cmd.NomeDestinatario, cmd.CpfCnpjDestinatario, cmd.EnderecoDestinatario,
            cmd.ValorTotal, cmd.NaturezaOperacao,
            cmd.Itens.Select(i => new NFeItemRequest(
                i.Descricao, i.CodigoProduto, i.Quantidade,
                i.ValorUnitario, i.Quantidade * i.ValorUnitario,
                i.Ncm, i.Cfop, i.UnidadeComercial)).ToList(),
            cmd.InformacoesAdicionais);

        var result = await nfeService.EmitirAsync(request, numeroNF, serie, ct);

        if (result.Sucesso)
        {
            nfe.Autorizar(result.ChaveAcesso!, result.Protocolo!, result.XmlAutorizado!, DateTime.UtcNow);
        }
        else
        {
            nfe.Rejeitar(result.MensagemErro ?? "Erro desconhecido");
        }

        await nfeRepository.UpdateAsync(nfe, ct);

        return new EmitirNFeResult(result.Sucesso, nfe.Id, result.ChaveAcesso, result.Protocolo, result.MensagemErro);
    }
}
