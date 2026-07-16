using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AMR.Financeiro.Application.Features.Parcelamentos.Commands;
using AMR.Financeiro.Application.Features.Parcelamentos.Queries;

namespace AMR.Financeiro.API.Controllers;

[Authorize]
[ApiController]
[Route("api/parcelamentos")]
public class ParcelamentosController(IMediator mediator) : ControllerBase
{
    // GET api/parcelamentos
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await mediator.Send(new GetParcelamentosQuery(), ct));

    // GET api/parcelamentos/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var dto = await mediator.Send(new GetParcelamentoByIdQuery(id), ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    // POST api/parcelamentos
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarParcelamentoCommand cmd, CancellationToken ct)
    {
        var id = await mediator.Send(cmd, ct);
        return Created($"/api/parcelamentos/{id}", new { id });
    }

    // PATCH api/parcelamentos/{id}/parcelas/{parcelaId}/pagar
    [HttpPatch("{id:int}/parcelas/{parcelaId:int}/pagar")]
    public async Task<IActionResult> PagarParcela(
        int id, int parcelaId, [FromBody] PagarParcelaPayload payload, CancellationToken ct)
    {
        var ok = await mediator.Send(
            new PagarParcelaCommand(id, parcelaId, payload.DataPagamento, payload.ContaBancariaId), ct);
        return ok ? NoContent() : NotFound();
    }
}

public record PagarParcelaPayload(DateTime DataPagamento, int? ContaBancariaId);
