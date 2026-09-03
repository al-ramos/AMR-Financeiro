using Microsoft.EntityFrameworkCore;
using AMR.Financeiro.Application.Interfaces;
using AMR.Financeiro.Domain.Entities;
using AMR.Financeiro.Domain.Enums;
using AMR.Financeiro.Infrastructure.Data;

namespace AMR.Financeiro.Infrastructure.Services;

/// <summary>
/// Matching automático por score ponderado (0-100):
/// valor (40), data (30), código do documento (15), descrição/favorecido (10).
/// Busca candidatos em ContasReceber e em Lancamentos numa janela de ±7 dias.
/// </summary>
public class ConciliacaoMatchingService(FinanceiroDbContext ctx) : IConciliacaoMatchingService
{
    private const int JanelaDias = 7;
    private const int ScoreAutoConciliar = 70;
    private const int MaxSugestoes = 5;

    public async Task<List<MatchSugestao>> BuscarSugestoesAsync(
        MovimentacaoBancaria movimentacao,
        CancellationToken ct = default)
    {
        var dataMin = movimentacao.DataLancamento.AddDays(-JanelaDias);
        var dataMax = movimentacao.DataLancamento.AddDays(JanelaDias);

        // Filial do extrato ao qual a movimentação pertence
        var extrato = await ctx.ExtratosBancarios
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == movimentacao.ExtratoId, ct);
        int? cdFilial = extrato?.CdFilial;

        // Lançamentos já vinculados a alguma movimentação conciliada não devem ser sugeridos de novo
        var idsJaConciliados = (await ctx.MovimentacoesBancarias
                .AsNoTracking()
                .Where(m => m.StatusConciliacao == StatusConciliacao.Conciliado && m.LancamentoId != null)
                .Select(m => m.LancamentoId!.Value)
                .ToListAsync(ct))
            .ToHashSet();

        var candidatos = new List<CandidatoMatch>();

        // 1) Contas a receber (não canceladas) — data de referência: Vencimento
        var contas = await ctx.ContasReceber
            .AsNoTracking()
            .Where(c => c.Status != StatusContaReceber.Cancelada
                     && c.Vencimento >= dataMin
                     && c.Vencimento <= dataMax
                     && (cdFilial == null || c.CdFilial == cdFilial))
            .ToListAsync(ct);

        candidatos.AddRange(contas.Select(c =>
            new CandidatoMatch(c.Id, c.Descricao, c.Valor, c.Vencimento, c.DocumentoOrigem)));

        // 2) Lançamentos financeiros — data de referência: DataLancamento
        var lancamentos = await ctx.Lancamentos
            .AsNoTracking()
            .Where(l => l.DataLancamento >= dataMin
                     && l.DataLancamento <= dataMax
                     && (cdFilial == null || l.CdFilial == cdFilial))
            .ToListAsync(ct);

        candidatos.AddRange(lancamentos.Select(l =>
            new CandidatoMatch(l.Id, l.Historico, l.Valor, l.DataLancamento, l.DocumentoOrigemId?.ToString())));

        return candidatos
            .Where(c => !idsJaConciliados.Contains(c.Id))
            .Select(c => new { Candidato = c, Score = CalcularScore(movimentacao, c) })
            .Where(x => x.Score > 0)
            .GroupBy(x => x.Candidato.Id)
            .Select(g => g.OrderByDescending(x => x.Score).First())
            .OrderByDescending(x => x.Score)
            .ThenBy(x => Math.Abs(x.Candidato.Data.DayNumber - movimentacao.DataLancamento.DayNumber))
            .Take(MaxSugestoes)
            .Select(x => new MatchSugestao(
                x.Candidato.Id,
                x.Candidato.Descricao,
                x.Candidato.Valor,
                x.Candidato.Data,
                x.Score,
                x.Score >= ScoreAutoConciliar))
            .ToList();
    }

    private static int CalcularScore(MovimentacaoBancaria movimentacao, CandidatoMatch lancamento)
    {
        var score = 0;

        // Valor (até 40 pontos)
        var diff = Math.Abs(movimentacao.Valor - lancamento.Valor);
        var pctDiff = lancamento.Valor > 0 ? diff / lancamento.Valor : 1;
        if (diff < 0.01m) score += 40;
        else if (pctDiff < 0.01m) score += 20;

        // Data (até 30 pontos)
        var diasDiff = Math.Abs(movimentacao.DataLancamento.DayNumber - lancamento.Data.DayNumber);
        if (diasDiff == 0) score += 30;
        else if (diasDiff <= 3) score += 15;

        // Código do documento (15 pontos)
        if (!string.IsNullOrEmpty(movimentacao.CodigoDoc) &&
            !string.IsNullOrEmpty(lancamento.Documento) &&
            lancamento.Documento.Contains(movimentacao.CodigoDoc, StringComparison.OrdinalIgnoreCase))
            score += 15;

        // Nome do favorecido na descrição (10 pontos)
        if (!string.IsNullOrEmpty(movimentacao.Descricao) &&
            !string.IsNullOrEmpty(lancamento.Descricao) &&
            movimentacao.Descricao.Contains(lancamento.Descricao.Split(' ')[0], StringComparison.OrdinalIgnoreCase))
            score += 10;

        return score;
    }

    private sealed record CandidatoMatch(int Id, string Descricao, decimal Valor, DateOnly Data, string? Documento);
}
