using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrojesDeMaranon.Application.Activities;
using TrojesDeMaranon.Application.Common;

namespace TrojesDeMaranon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/activity-catalog")]
public sealed class ActivityCatalogController(IMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ActivityCatalogDto>>>> GetAll(CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetActivityCatalogQuery(RequireCompanyId()), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ActivityCatalogDto>>> Get(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetActivityByIdQuery(RequireCompanyId(), id), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ActivityCatalogDto>>> Create(UpsertActivityCatalogRequest request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new CreateActivityCatalogCommand(RequireCompanyId(), request), cancellationToken);
        return response.Success && response.Data is not null
            ? this.ToCreatedAtActionResult(response, nameof(Get), new { id = response.Data.Id })
            : this.ToActionResult(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ActivityCatalogDto>>> Update(Guid id, UpsertActivityCatalogRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new UpdateActivityCatalogCommand(RequireCompanyId(), id, request), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new DeleteActivityCatalogCommand(RequireCompanyId(), id), cancellationToken));

    private Guid RequireCompanyId() => currentUser.CompanyId ?? throw new UnauthorizedAccessException("Sesion sin empresa.");
}
