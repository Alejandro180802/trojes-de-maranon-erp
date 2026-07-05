using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Application.Inventory;

namespace TrojesDeMaranon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/material-issues")]
public sealed class MaterialIssuesController(IMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MaterialIssueDto>>>> GetAll(CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetMaterialIssuesQuery(RequireCompanyId()), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<MaterialIssueDto>>> Get(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetMaterialIssueByIdQuery(RequireCompanyId(), id), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MaterialIssueDto>>> Create(UpsertMaterialIssueRequest request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new CreateMaterialIssueCommand(RequireCompanyId(), request), cancellationToken);
        return response.Success && response.Data is not null ? this.ToCreatedAtActionResult(response, nameof(Get), new { id = response.Data.Id }) : this.ToActionResult(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<MaterialIssueDto>>> Update(Guid id, UpsertMaterialIssueRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new UpdateMaterialIssueCommand(RequireCompanyId(), id, request), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new DeleteMaterialIssueCommand(RequireCompanyId(), id), cancellationToken));

    [HttpPost("{id:guid}/post")]
    public async Task<ActionResult<ApiResponse<MaterialIssueDto>>> Post(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new PostMaterialIssueCommand(RequireCompanyId(), id, currentUser.UserId), cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<MaterialIssueDto>>> Cancel(Guid id, CancelDocumentRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new CancelMaterialIssueCommand(RequireCompanyId(), id, currentUser.UserId, request), cancellationToken));

    private Guid RequireCompanyId() => currentUser.CompanyId ?? throw new UnauthorizedAccessException("Sesion sin empresa.");
}
