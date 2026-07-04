using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Application.Materials;

namespace TrojesDeMaranon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/material-subfamilies")]
public sealed class MaterialSubfamiliesController(IMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MaterialSubfamilyDto>>>> GetAll(CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetMaterialSubfamiliesQuery(RequireCompanyId()), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<MaterialSubfamilyDto>>> Get(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetMaterialSubfamilyByIdQuery(RequireCompanyId(), id), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MaterialSubfamilyDto>>> Create(UpsertMaterialSubfamilyRequest request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new CreateMaterialSubfamilyCommand(RequireCompanyId(), request), cancellationToken);
        return response.Success && response.Data is not null
            ? this.ToCreatedAtActionResult(response, nameof(Get), new { id = response.Data.Id })
            : this.ToActionResult(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<MaterialSubfamilyDto>>> Update(Guid id, UpsertMaterialSubfamilyRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new UpdateMaterialSubfamilyCommand(RequireCompanyId(), id, request), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new DeleteMaterialSubfamilyCommand(RequireCompanyId(), id), cancellationToken));

    private Guid RequireCompanyId() => currentUser.CompanyId ?? throw new UnauthorizedAccessException("Sesion sin empresa.");
}
