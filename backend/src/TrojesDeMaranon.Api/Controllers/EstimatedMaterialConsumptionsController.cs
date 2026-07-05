using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Application.Projects;

namespace TrojesDeMaranon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/estimated-material-consumptions")]
public sealed class EstimatedMaterialConsumptionsController(IMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<EstimatedMaterialConsumptionDto>>> Update(Guid id, UpsertEstimatedMaterialConsumptionRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new UpdateEstimatedMaterialConsumptionCommand(RequireCompanyId(), id, request), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new DeleteEstimatedMaterialConsumptionCommand(RequireCompanyId(), id), cancellationToken));

    private Guid RequireCompanyId() => currentUser.CompanyId ?? throw new UnauthorizedAccessException("Sesion sin empresa.");
}
