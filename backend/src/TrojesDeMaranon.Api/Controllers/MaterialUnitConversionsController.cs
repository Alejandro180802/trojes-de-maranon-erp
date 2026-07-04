using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Application.Materials;

namespace TrojesDeMaranon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/material-unit-conversions")]
public sealed class MaterialUnitConversionsController(IMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<MaterialUnitConversionDto>>> Update(Guid id, UpsertMaterialUnitConversionRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new UpdateMaterialUnitConversionCommand(RequireCompanyId(), id, request), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new DeleteMaterialUnitConversionCommand(RequireCompanyId(), id), cancellationToken));

    private Guid RequireCompanyId() => currentUser.CompanyId ?? throw new UnauthorizedAccessException("Sesion sin empresa.");
}
