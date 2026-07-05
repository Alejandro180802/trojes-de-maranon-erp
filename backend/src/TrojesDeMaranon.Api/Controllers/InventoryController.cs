using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Application.Inventory;

namespace TrojesDeMaranon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/inventory")]
public sealed class InventoryController(IMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet("balances")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InventoryBalanceDto>>>> Balances([FromQuery] Guid? warehouseId, [FromQuery] Guid? materialId, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetInventoryBalancesQuery(RequireCompanyId(), warehouseId, materialId), cancellationToken));

    [HttpGet("movements")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InventoryMovementDto>>>> Movements([FromQuery] Guid? warehouseId, [FromQuery] Guid? materialId, [FromQuery] Guid? projectId, [FromQuery] Guid? platformId, [FromQuery] string? movementType, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetInventoryMovementsQuery(RequireCompanyId(), warehouseId, materialId, projectId, platformId, movementType, dateFrom, dateTo), cancellationToken));

    private Guid RequireCompanyId() => currentUser.CompanyId ?? throw new UnauthorizedAccessException("Sesion sin empresa.");
}
