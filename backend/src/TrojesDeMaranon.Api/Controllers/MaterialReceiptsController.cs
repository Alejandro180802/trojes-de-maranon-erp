using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Application.Inventory;

namespace TrojesDeMaranon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/material-receipts")]
public sealed class MaterialReceiptsController(IMediator mediator, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MaterialReceiptDto>>>> GetAll(CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetMaterialReceiptsQuery(RequireCompanyId()), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<MaterialReceiptDto>>> Get(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetMaterialReceiptByIdQuery(RequireCompanyId(), id), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MaterialReceiptDto>>> Create(UpsertMaterialReceiptRequest request, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new CreateMaterialReceiptCommand(RequireCompanyId(), request), cancellationToken);
        return response.Success && response.Data is not null ? this.ToCreatedAtActionResult(response, nameof(Get), new { id = response.Data.Id }) : this.ToActionResult(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<MaterialReceiptDto>>> Update(Guid id, UpsertMaterialReceiptRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new UpdateMaterialReceiptCommand(RequireCompanyId(), id, request), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new DeleteMaterialReceiptCommand(RequireCompanyId(), id), cancellationToken));

    [HttpPost("{id:guid}/post")]
    public async Task<ActionResult<ApiResponse<MaterialReceiptDto>>> Post(Guid id, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new PostMaterialReceiptCommand(RequireCompanyId(), id, currentUser.UserId), cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<MaterialReceiptDto>>> Cancel(Guid id, CancelDocumentRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new CancelMaterialReceiptCommand(RequireCompanyId(), id, currentUser.UserId, request), cancellationToken));

    private Guid RequireCompanyId() => currentUser.CompanyId ?? throw new UnauthorizedAccessException("Sesion sin empresa.");
}
