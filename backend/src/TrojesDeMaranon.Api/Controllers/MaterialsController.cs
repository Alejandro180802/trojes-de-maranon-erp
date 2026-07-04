using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Application.Materials;

namespace TrojesDeMaranon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/materials")]
public sealed class MaterialsController(IMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MaterialDto>>>> GetAll(CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetMaterialsQuery(RequireCompanyId()), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<MaterialDto>>> Get(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetMaterialByIdQuery(RequireCompanyId(), id), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MaterialDto>>> Create(UpsertMaterialRequest request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new CreateMaterialCommand(RequireCompanyId(), request), cancellationToken);
        return response.Success && response.Data is not null
            ? this.ToCreatedAtActionResult(response, nameof(Get), new { id = response.Data.Id })
            : this.ToActionResult(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<MaterialDto>>> Update(Guid id, UpsertMaterialRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new UpdateMaterialCommand(RequireCompanyId(), id, request), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new DeleteMaterialCommand(RequireCompanyId(), id), cancellationToken));

    [HttpGet("{id:guid}/unit-conversions")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MaterialUnitConversionDto>>>> GetConversions(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetMaterialUnitConversionsQuery(RequireCompanyId(), id), cancellationToken));

    [HttpPost("{id:guid}/unit-conversions")]
    public async Task<ActionResult<ApiResponse<MaterialUnitConversionDto>>> CreateConversion(Guid id, UpsertMaterialUnitConversionRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new CreateMaterialUnitConversionCommand(RequireCompanyId(), id, request), cancellationToken));

    private Guid RequireCompanyId() => currentUser.CompanyId ?? throw new UnauthorizedAccessException("Sesion sin empresa.");
}
