using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AMR.Financeiro.Application.Features.Conciliacao.Commands;
using AMR.Financeiro.Application.Features.Conciliacao.Queries;

namespace AMR.Financeiro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ConciliacaoController(IMediator mediator) : ControllerBase
{
    // POST api/conciliacao/importar
    [HttpPost("importar")]
    public async Task<IActionResult> Importar([FromBody] ImportarExtratoRequest req, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(
                new ImportarExtratoCommand(req.CdFilial, req.ArquivoNome, req.ArquivoConteudo), ct);
            return CreatedAtAction(nameof(GetExtrato), new { extratoId = result.ExtratoId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { erro = ex.Message });
        }
        catch (NotSupportedException ex)
        {
            return UnprocessableEntity(new { erro = ex.Message });
        }
    }

    // GET api/conciliacao/extrato/5
    [HttpGet("extrato/{extratoId:int}")]
    public async Task<IActionResult> GetExtrato(int extratoId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetExtratoQuery(extratoId), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // POST api/conciliacao/conciliar-manual
    [HttpPost("conciliar-manual")]
    public async Task<IActionResult> ConciliarManual([FromBody] ConciliarManualCommand cmd, CancellationToken ct)
    {
        var sucesso = await mediator.Send(cmd, ct);
        return sucesso ? Ok(new { sucesso }) : NotFound();
    }

    // POST api/conciliacao/ignorar
    [HttpPost("ignorar")]
    public async Task<IActionResult> Ignorar([FromBody] IgnorarMovimentacaoCommand cmd, CancellationToken ct)
    {
        var sucesso = await mediator.Send(cmd, ct);
        return sucesso ? Ok(new { sucesso }) : NotFound();
    }

    // GET api/conciliacao/pendentes?cdFilial=1&diasPassados=60
    [HttpGet("pendentes")]
    public async Task<IActionResult> GetPendentes(
        [FromQuery] int cdFilial, [FromQuery] int diasPassados = 60, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetPendentesQuery(cdFilial, diasPassados), ct);
        return Ok(result);
    }
}

public record ImportarExtratoRequest(int CdFilial, string ArquivoNome, string ArquivoConteudo);
