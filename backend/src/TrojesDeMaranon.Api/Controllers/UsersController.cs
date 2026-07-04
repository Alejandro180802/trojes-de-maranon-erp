using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrojesDeMaranon.Application.Common;
using TrojesDeMaranon.Application.Roles;
using TrojesDeMaranon.Application.Users;
using TrojesDeMaranon.Domain.Security;
using TrojesDeMaranon.Persistence.Context;

namespace TrojesDeMaranon.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/users")]
public sealed class UsersController(AppDbContext db, IValidator<CreateUserRequest> createValidator, IValidator<UpdateUserRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<IReadOnlyList<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await db.Users.AsNoTracking()
            .OrderBy(x => x.FullName)
            .Select(x => new UserDto(x.Id, x.CompanyId, x.BranchId, x.FullName, x.Email, x.Phone, x.IsActive))
            .ToListAsync(cancellationToken);
        return ApiResponse<IReadOnlyList<UserDto>>.Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Get(Guid id, CancellationToken cancellationToken)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return user is null ? NotFound(ApiResponse<UserDto>.Fail("Usuario no encontrado.")) : ApiResponse<UserDto>.Ok(ToDto(user));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        if (!await db.Companies.AnyAsync(x => x.Id == request.CompanyId, cancellationToken))
        {
            return BadRequest(ApiResponse<UserDto>.Fail("Empresa invalida."));
        }
        var user = new User
        {
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            IsActive = true
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, ApiResponse<UserDto>.Ok(ToDto(user)));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound(ApiResponse<UserDto>.Fail("Usuario no encontrado."));
        }
        user.BranchId = request.BranchId;
        user.FullName = request.FullName;
        user.Phone = request.Phone;
        user.IsActive = request.IsActive;
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<UserDto>.Ok(ToDto(user));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound(ApiResponse<object>.Fail("Usuario no encontrado."));
        }
        db.Users.Remove(user);
        await db.SaveChangesAsync(cancellationToken);
        return ApiResponse<object>.Ok(new { });
    }

    [HttpPost("{id:guid}/roles")]
    public async Task<ActionResult<ApiResponse<object>>> AssignRole(Guid id, AssignRoleRequest request, CancellationToken cancellationToken)
    {
        var exists = await db.Users.AnyAsync(x => x.Id == id, cancellationToken);
        var roleExists = await db.Roles.AnyAsync(x => x.Id == request.RoleId, cancellationToken);
        if (!exists || !roleExists)
        {
            return BadRequest(ApiResponse<object>.Fail("Usuario o rol invalido."));
        }
        if (!await db.UserRoles.AnyAsync(x => x.UserId == id && x.RoleId == request.RoleId, cancellationToken))
        {
            db.UserRoles.Add(new UserRole { UserId = id, RoleId = request.RoleId });
            await db.SaveChangesAsync(cancellationToken);
        }
        return ApiResponse<object>.Ok(new { });
    }

    private static UserDto ToDto(User user) => new(user.Id, user.CompanyId, user.BranchId, user.FullName, user.Email, user.Phone, user.IsActive);
}
