using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Application.Inventory;

namespace TrojesDeMaranon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/inventory-adjustments")]
public sealed class InventoryAdjustmentsController(IMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InventoryAdjustmentDto>>>> GetAll(CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetInventoryAdjustmentsQuery(RequireCompanyId()), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<InventoryAdjustmentDto>>> Get(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetInventoryAdjustmentByIdQuery(RequireCompanyId(), id), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<InventoryAdjustmentDto>>> Create(UpsertInventoryAdjustmentRequest request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new CreateInventoryAdjustmentCommand(RequireCompanyId(), request), cancellationToken);
        return response.Success && response.Data is not null ? this.ToCreatedAtActionResult(response, nameof(Get), new { id = response.Data.Id }) : this.ToActionResult(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<InventoryAdjustmentDto>>> Update(Guid id, UpsertInventoryAdjustmentRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new UpdateInventoryAdjustmentCommand(RequireCompanyId(), id, request), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new DeleteInventoryAdjustmentCommand(RequireCompanyId(), id), cancellationToken));

    [HttpPost("{id:guid}/post")]
    public async Task<ActionResult<ApiResponse<InventoryAdjustmentDto>>> Post(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new PostInventoryAdjustmentCommand(RequireCompanyId(), id, currentUser.UserId), cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<InventoryAdjustmentDto>>> Cancel(Guid id, CancelDocumentRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new CancelInventoryAdjustmentCommand(RequireCompanyId(), id, currentUser.UserId, request), cancellationToken));

    private Guid RequireCompanyId() => currentUser.CompanyId ?? throw new UnauthorizedAccessException("Sesion sin empresa.");
}
