using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AMR.Financeiro.Application.Features.CentroCusto.Commands;
using AMR.Financeiro.Application.Features.CentroCusto.Queries;

namespace AMR.Financeiro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/centros-custo")]
public class CentrosCustoController(IMediator mediator) : ControllerBase
{
    // GET api/centros-custo?cdFilial=1
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int cdFilial, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCentrosCustoQuery(cdFilial), ct);
        return Ok(result);
    }

    // POST api/centros-custo
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarCentroCustoCommand cmd, CancellationToken ct)
    {
        try
        {
            var id = await mediator.Send(cmd, ct);
            return Created($"/api/centros-custo/{id}", new { id });
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentOutOfRangeException)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    // GET api/centros-custo/5/orcamento?ano=2026
    [HttpGet("{id:int}/orcamento")]
    public async Task<IActionResult> GetOrcamento(int id, [FromQuery] int ano, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrcamentoQuery(id, ano), ct);
        return Ok(result);
    }

    // PUT api/centros-custo/orcamento
    [HttpPut("orcamento")]
    public async Task<IActionResult> AtualizarOrcamento([FromBody] AtualizarOrcamentoCommand cmd, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(cmd, ct);
            return Ok(new { sucesso = result });
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentOutOfRangeException)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    // GET api/centros-custo/alertas?cdFilial=1
    [HttpGet("alertas")]
    public async Task<IActionResult> GetAlertas([FromQuery] int cdFilial, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAlertasQuery(cdFilial), ct);
        return Ok(result);
    }

    // POST api/centros-custo/rateio/regras
    [HttpPost("rateio/regras")]
    public async Task<IActionResult> CriarRegraRateio([FromBody] CriarRegraRateioCommand cmd, CancellationToken ct)
    {
        try
        {
            var id = await mediator.Send(cmd, ct);
            return Created($"/api/centros-custo/rateio/regras/{id}", new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    // POST api/centros-custo/rateio/executar?cdFilial=1&ano=2026&mes=7
    [HttpPost("rateio/executar")]
    public async Task<IActionResult> ExecutarRateio(
        [FromQuery] int cdFilial, [FromQuery] int ano, [FromQuery] int mes, CancellationToken ct)
    {
        try
        {
            var result = await mediator.Send(new ExecutarRateioCommand(cdFilial, ano, mes), ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            // Rateio da competência já executado
            return Conflict(new { erro = ex.Message });
        }
    }
}
