using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Application.Inventory;

namespace TrojesDeMaranon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/inventory-transfers")]
public sealed class InventoryTransfersController(IMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InventoryTransferDto>>>> GetAll(CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetInventoryTransfersQuery(RequireCompanyId()), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<InventoryTransferDto>>> Get(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetInventoryTransferByIdQuery(RequireCompanyId(), id), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<InventoryTransferDto>>> Create(UpsertInventoryTransferRequest request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new CreateInventoryTransferCommand(RequireCompanyId(), request), cancellationToken);
        return response.Success && response.Data is not null ? this.ToCreatedAtActionResult(response, nameof(Get), new { id = response.Data.Id }) : this.ToActionResult(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<InventoryTransferDto>>> Update(Guid id, UpsertInventoryTransferRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new UpdateInventoryTransferCommand(RequireCompanyId(), id, request), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new DeleteInventoryTransferCommand(RequireCompanyId(), id), cancellationToken));

    [HttpPost("{id:guid}/post")]
    public async Task<ActionResult<ApiResponse<InventoryTransferDto>>> Post(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new PostInventoryTransferCommand(RequireCompanyId(), id, currentUser.UserId), cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<InventoryTransferDto>>> Cancel(Guid id, CancelDocumentRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new CancelInventoryTransferCommand(RequireCompanyId(), id, currentUser.UserId, request), cancellationToken));

    private Guid RequireCompanyId() => currentUser.CompanyId ?? throw new UnauthorizedAccessException("Sesion sin empresa.");
}
