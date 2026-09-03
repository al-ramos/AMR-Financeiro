using System.Security.Cryptography;
using System.Text;
using MediatR;
using AMR.Financeiro.Application.Interfaces;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Domain.Interfaces;

namespace AMR.Financeiro.Application.Features.Conciliacao.Commands;

public record ImportarExtratoCommand(
    int CdFilial,
    string ArquivoNome,
    string ArquivoConteudo) : IRequest<ImportarExtratoResult>;

public record ImportarExtratoResult(
    int ExtratoId,
    string Banco,
    string Periodo,
    int TotalMovimentacoes,
    int ConciliadosAutomaticamente,
    int Pendentes,
    int Divergentes);

public class ImportarExtratoCommandHandler(
    IConciliacaoRepository repo,
    IEnumerable<IExtratoParser> parsers,
    IConciliacaoMatchingService matchingService,
    IUnitOfWork uow) : IRequestHandler<ImportarExtratoCommand, ImportarExtratoResult>
{
    /// <summary>Score mínimo para marcar como divergente (possível match que exige revisão manual).</summary>
    private const int ScoreDivergente = 40;

    public async Task<ImportarExtratoResult> Handle(ImportarExtratoCommand cmd, CancellationToken ct)
    {
        var conteudo = NormalizarConteudo(cmd.ArquivoConteudo);

        // 1. Idempotência via SHA256
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(conteudo)));
        if (await repo.ExtratoJaImportadoAsync(cmd.CdFilial, hash, ct))
            throw new InvalidOperationException("Extrato já importado anteriormente (arquivo duplicado).");

        // 2/3. Detecta formato e faz o parse
        var parser = parsers.FirstOrDefault(p => p.Suporta(conteudo))
            ?? throw new NotSupportedException("Formato de extrato não reconhecido (esperado OFX ou CNAB 240).");

        var formato = ConteudoEhOfx(conteudo) ? FormatoExtrato.OFX : FormatoExtrato.CNAB240;
        var resultado = parser.Parse(conteudo);

        // 4. Cria o extrato
        var extrato = new ExtratoBancario(
            cmd.CdFilial,
            resultado.Banco,
            resultado.ContaCorrente,
            resultado.DataInicio,
            resultado.DataFim,
            resultado.SaldoInicial,
            resultado.SaldoFinal,
            resultado.Movimentacoes.Where(m => m.Tipo == TipoMovimentacao.Credito).Sum(m => m.Valor),
            resultado.Movimentacoes.Where(m => m.Tipo == TipoMovimentacao.Debito).Sum(m => m.Valor),
            formato,
            cmd.ArquivoNome);

        await repo.AddExtratoAsync(extrato, ct);
        await uow.SaveChangesAsync(ct); // garante extrato.Id para o FK das movimentações

        // 5/6. Movimentações + matching automático
        int conciliadosAuto = 0, divergentes = 0;
        foreach (var item in resultado.Movimentacoes)
        {
            var mov = new MovimentacaoBancaria(
                extrato.Id, item.DataLancamento, item.Tipo, item.Valor, item.Descricao, item.CodigoDoc);

            var sugestoes = await matchingService.BuscarSugestoesAsync(mov, ct);
            var melhor = sugestoes.FirstOrDefault();

            if (melhor is { AutoConciliar: true })
            {
                mov.ConciliarCom(melhor.LancamentoId, "Auto");
                conciliadosAuto++;
            }
            else if (melhor is not null && melhor.Score >= ScoreDivergente)
            {
                mov.MarcarDivergente();
                divergentes++;
            }

            await repo.AddMovimentacaoAsync(mov, ct);
        }

        // 7. Persiste movimentações e hash de idempotência
        await uow.SaveChangesAsync(ct);
        await repo.SalvarHashAsync(cmd.CdFilial, hash, extrato.Id, ct);
        await uow.SaveChangesAsync(ct);

        var total = resultado.Movimentacoes.Count;
        return new ImportarExtratoResult(
            extrato.Id,
            extrato.Banco,
            $"{extrato.DataInicio:dd/MM/yyyy} a {extrato.DataFim:dd/MM/yyyy}",
            total,
            conciliadosAuto,
            total - conciliadosAuto - divergentes,
            divergentes);
    }

    private static bool ConteudoEhOfx(string conteudo) =>
        conteudo.Contains("OFXHEADER:", StringComparison.OrdinalIgnoreCase) ||
        conteudo.Contains("<OFX>", StringComparison.OrdinalIgnoreCase);

    private static bool ConteudoEhCnab240(string conteudo)
    {
        var primeira = conteudo.Split('\n')[0].TrimEnd('\r');
        return primeira.Length == 240;
    }

    /// <summary>Aceita texto puro ou Base64 — se não reconhecer o texto, tenta decodificar Base64.</summary>
    private static string NormalizarConteudo(string conteudo)
    {
        if (ConteudoEhOfx(conteudo) || ConteudoEhCnab240(conteudo))
            return conteudo;

        var compacto = conteudo.Replace("\r", "").Replace("\n", "").Trim();
        var buffer = new byte[compacto.Length];
        if (Convert.TryFromBase64String(compacto, buffer, out var bytesEscritos))
        {
            var decodificado = Encoding.UTF8.GetString(buffer, 0, bytesEscritos);
            if (ConteudoEhOfx(decodificado) || ConteudoEhCnab240(decodificado))
                return decodificado;
        }

        return conteudo;
    }
}
