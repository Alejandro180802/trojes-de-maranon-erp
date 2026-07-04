namespace TrojesDeMaranon.Application.Roles;

public sealed record RoleDto(Guid Id, Guid CompanyId, string Name, string? Description, bool IsSystemRole);
public sealed record CreateRoleRequest(Guid CompanyId, string Name, string? Description);
public sealed record UpdateRoleRequest(string Name, string? Description);
public sealed record AssignRoleRequest(Guid RoleId);
public sealed record AssignPermissionRequest(Guid PermissionId);
