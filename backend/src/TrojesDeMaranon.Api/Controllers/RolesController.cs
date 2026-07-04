using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Application.Roles;
using TrojesDeMaranon.Domain.Security;
using TrojesDeMaranon.Persistence.Context;

namespace TrojesDeMaranon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/roles")]
public sealed class RolesController(AppDbContext db, IValidator<CreateRoleRequest> createValidator, IValidator<UpdateRoleRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<IReadOnlyList<RoleDto>>> GetAll(CancellationToken cancellationToken)
    {
        var roles = await db.Roles.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new RoleDto(x.Id, x.CompanyId, x.Name, x.Description, x.IsSystemRole))
            .ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<RoleDto>>.Ok(roles);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var role = new Role { CompanyId = request.CompanyId, Name = request.Name, Description = request.Description };
        db.Roles.Add(role);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<RoleDto>.Ok(new RoleDto(role.Id, role.CompanyId, role.Name, role.Description, role.IsSystemRole));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Update(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        var role = await db.Roles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (role is null)
        {
            return NotFound(ApiResponse<RoleDto>.Fail("Rol no encontrado."));
        }
        role.Name = request.Name;
        role.Description = request.Description;
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<RoleDto>.Ok(new RoleDto(role.Id, role.CompanyId, role.Name, role.Description, role.IsSystemRole));
    }

    [HttpPost("{id:guid}/permissions")]
    public async Task<ActionResult<ApiResponse<object>>> AssignPermission(Guid id, AssignPermissionRequest request, CancellationToken cancellationToken)
    {
        var exists = await db.Roles.AnyAsync(x => x.Id == id, cancellationToken);
        var permissionExists = await db.Permissions.AnyAsync(x => x.Id == request.PermissionId, cancellationToken);
        if (!exists || !permissionExists)
        {
            return BadRequest(ApiResponse<object>.Fail("Rol o permiso invalido."));
        }
        if (!await db.RolePermissions.AnyAsync(x => x.RoleId == id && x.PermissionId == request.PermissionId, cancellationToken))
        {
            db.RolePermissions.Add(new RolePermission { RoleId = id, PermissionId = request.PermissionId });
            await db.SaveChangesAsync(cancellationToken);
        }
        return ApiResponse<object>.Ok(new { });
    }
}
