using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Application.Projects;

namespace TrojesDeMaranon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/platform-activities")]
public sealed class PlatformActivitiesController(IMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PlatformActivityDto>>> Update(Guid id, UpsertPlatformActivityRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new UpdatePlatformActivityCommand(RequireCompanyId(), id, request), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new DeletePlatformActivityCommand(RequireCompanyId(), id), cancellationToken));

    [HttpPatch("{id:guid}/progress")]
    public async Task<ActionResult<ApiResponse<PlatformActivityDto>>> Progress(Guid id, UpdatePlatformActivityProgressRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new UpdatePlatformActivityProgressCommand(RequireCompanyId(), id, request), cancellationToken));

    private Guid RequireCompanyId() => currentUser.CompanyId ?? throw new UnauthorizedAccessException("Sesion sin empresa.");
}
