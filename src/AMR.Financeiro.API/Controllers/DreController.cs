using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AMR.Financeiro.API.Helpers;
using AMR.Financeiro.Application.Features.DRE.Queries;
using AMR.Financeiro.Application.Features.PlanoDeContas.Commands;
using AMR.Financeiro.Application.Features.PlanoDeContas.Queries;
using AMR.Financeiro.Domain.Enums;

namespace AMR.Financeiro.API.Controllers;

/// <summary>
/// Plano de Contas gerencial (5 níveis) + DRE — Card 23.4.
/// Rotas em /api/plano-contas e /api/dre (o legado continua em /api/planocontas).
/// </summary>
[Authorize]
[ApiController]
[Route("api")]
public class DreController(IMediator mediator) : ControllerBase
{
    // ── Plano de Contas ───────────────────────────────────────────────────────

    // GET api/plano-contas?cdFilial=1&incluirInativos=false
    [HttpGet("plano-contas")]
    public async Task<IActionResult> GetPlanoContas(
        [FromQuery] int cdFilial, [FromQuery] bool incluirInativos, CancellationToken ct)
    {
        var result = await mediator.Send(new GetPlanoContasQuery(cdFilial, incluirInativos), ct);
        return Ok(result);
    }

    // POST api/plano-contas
    [HttpPost("plano-contas")]
    public async Task<IActionResult> Criar([FromBody] CriarContaCommand cmd, CancellationToken ct)
    {
        try
        {
            var id = await mediator.Send(cmd, ct);
            return Created($"/api/plano-contas/{id}", new { id });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { erro = ex.Message });
        }
    }

    // PUT api/plano-contas/5
    [HttpPut("plano-contas/{id:int}")]
    public async Task<IActionResult> Atualizar(
        int id, [FromBody] AtualizarContaRequest req, CancellationToken ct)
    {
        var ok = await mediator.Send(
            new AtualizarContaCommand(id, req.Descricao, req.GrupoDre, req.OrdemExibicao), ct);
        return ok ? Ok(new { id }) : NotFound();
    }

    // DELETE api/plano-contas/5 — inativação lógica; 409 se houver lançamentos ou filhas
    [HttpDelete("plano-contas/{id:int}")]
    public async Task<IActionResult> Inativar(int id, CancellationToken ct)
    {
        try
        {
            var ok = await mediator.Send(new InativarContaCommand(id), ct);
            return ok ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { erro = ex.Message });
        }
    }

    // POST api/plano-contas/seed?cdFilial=1 — plano padrão CFC (desenvolvimento/setup inicial)
    [HttpPost("plano-contas/seed")]
    public async Task<IActionResult> Seed([FromQuery] int cdFilial, CancellationToken ct)
    {
        var contasCriadas = await mediator.Send(new SeedPlanoContasCommand(cdFilial), ct);
        return Created($"/api/plano-contas?cdFilial={cdFilial}", new { contasCriadas });
    }

    // ── DRE ───────────────────────────────────────────────────────────────────

    // GET api/dre?cdFilial=1&ano=2026&mes=7
    [HttpGet("dre")]
    public async Task<IActionResult> GetDre(
        [FromQuery] int cdFilial, [FromQuery] int ano, [FromQuery] int mes, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new GetDreQuery(cdFilial, ano, mes), ct);
            return Ok(result);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    // GET api/dre/export?cdFilial=1&ano=2026&mes=7 — CSV (;) pt-BR, abre direto no Excel
    [HttpGet("dre/export")]
    public async Task<IActionResult> ExportDre(
        [FromQuery] int cdFilial, [FromQuery] int ano, [FromQuery] int mes, CancellationToken ct)
    {
        try
        {
            var dre = await mediator.Send(new GetDreQuery(cdFilial, ano, mes), ct);

            var rows = dre.Linhas.Select(l => new DreExportRow(
                l.Descricao,
                l.ValorAtual,
                l.ValorPeriodoAnterior,
                l.ValorMesmoMesAnoAnterior,
                l.VariacaoMes,
                l.VariacaoAno));

            return CsvExportHelper.Export(rows, $"dre_{cdFilial}_{ano}_{mes:D2}");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }
}

public record AtualizarContaRequest(string Descricao, GrupoDRE GrupoDre, int OrdemExibicao);

public record DreExportRow(
    string Linha,
    decimal ValorAtual,
    decimal ValorPeriodoAnterior,
    decimal ValorMesmoMesAnoAnterior,
    decimal VariacaoMesPercentual,
    decimal VariacaoAnoPercentual);
