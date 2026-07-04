using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Application.Permissions;
using TrojesDeMaranon.Persistence.Context;

namespace TrojesDeMaranon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/permissions")]
public sealed class PermissionsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<IReadOnlyList<PermissionDto>>> GetAll(CancellationToken cancellationToken)
    {
        var permissions = await db.Permissions.AsNoTracking()
            .OrderBy(x => x.Module).ThenBy(x => x.Action)
            .Select(x => new PermissionDto(x.Id, x.Module, x.Action, x.Code, x.Description))
            .ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<PermissionDto>>.Ok(permissions);
    }
}
