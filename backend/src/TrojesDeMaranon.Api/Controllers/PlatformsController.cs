using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Application.Projects;

namespace TrojesDeMaranon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/platforms")]
public sealed class PlatformsController(IMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PlatformDto>>>> GetAll([FromQuery] Guid? projectId, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetPlatformsQuery(RequireCompanyId(), projectId), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PlatformDto>>> Get(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetPlatformByIdQuery(RequireCompanyId(), id), cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PlatformDto>>> Update(Guid id, UpsertPlatformRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new UpdatePlatformCommand(RequireCompanyId(), id, request), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new DeletePlatformCommand(RequireCompanyId(), id), cancellationToken));

    [HttpPatch("{id:guid}/progress")]
    public async Task<ActionResult<ApiResponse<PlatformDto>>> Progress(Guid id, UpdatePlatformProgressRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new UpdatePlatformProgressCommand(RequireCompanyId(), id, request), cancellationToken));

    [HttpGet("{platformId:guid}/activities")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PlatformActivityDto>>>> Activities(Guid platformId, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetPlatformActivitiesQuery(RequireCompanyId(), platformId), cancellationToken));

    [HttpPost("{platformId:guid}/activities")]
    public async Task<ActionResult<ApiResponse<PlatformActivityDto>>> CreateActivity(Guid platformId, UpsertPlatformActivityRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new CreatePlatformActivityCommand(RequireCompanyId(), platformId, request), cancellationToken));

    [HttpGet("{platformId:guid}/estimated-material-consumptions")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EstimatedMaterialConsumptionDto>>>> EstimatedMaterialConsumptions(Guid platformId, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetEstimatedMaterialConsumptionsQuery(RequireCompanyId(), platformId), cancellationToken));

    [HttpPost("{platformId:guid}/estimated-material-consumptions")]
    public async Task<ActionResult<ApiResponse<EstimatedMaterialConsumptionDto>>> CreateEstimatedMaterialConsumption(Guid platformId, UpsertEstimatedMaterialConsumptionRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new CreateEstimatedMaterialConsumptionCommand(RequireCompanyId(), platformId, request), cancellationToken));

    private Guid RequireCompanyId() => currentUser.CompanyId ?? throw new UnauthorizedAccessException("Sesion sin empresa.");
}
