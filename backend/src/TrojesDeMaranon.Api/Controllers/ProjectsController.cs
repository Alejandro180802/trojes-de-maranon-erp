using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Application.Projects;

namespace TrojesDeMaranon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/projects")]
public sealed class ProjectsController(IMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProjectDto>>>> GetAll(CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetProjectsQuery(RequireCompanyId()), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> Get(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetProjectByIdQuery(RequireCompanyId(), id), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> Create(UpsertProjectRequest request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new CreateProjectCommand(RequireCompanyId(), request), cancellationToken);
        return response.Success && response.Data is not null
            ? this.ToCreatedAtActionResult(response, nameof(Get), new { id = response.Data.Id })
            : this.ToActionResult(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> Update(Guid id, UpsertProjectRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new UpdateProjectCommand(RequireCompanyId(), id, request), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new DeleteProjectCommand(RequireCompanyId(), id), cancellationToken));

    [HttpGet("{id:guid}/summary")]
    public async Task<ActionResult<ApiResponse<ProjectSummaryDto>>> Summary(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetProjectSummaryQuery(RequireCompanyId(), id), cancellationToken));

    [HttpGet("{projectId:guid}/platforms")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PlatformDto>>>> Platforms(Guid projectId, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetProjectPlatformsQuery(RequireCompanyId(), projectId), cancellationToken));

    [HttpPost("{projectId:guid}/platforms")]
    public async Task<ActionResult<ApiResponse<PlatformDto>>> CreatePlatform(Guid projectId, UpsertPlatformRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await mediator.Send(new CreatePlatformCommand(RequireCompanyId(), projectId, request), cancellationToken));

    private Guid RequireCompanyId() => currentUser.CompanyId ?? throw new UnauthorizedAccessException("Sesion sin empresa.");
}
