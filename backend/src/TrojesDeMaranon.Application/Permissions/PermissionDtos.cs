namespace TrojesDeMaranon.Application.Permissions;

public sealed record PermissionDto(Guid Id, string Module, string Action, string Code, string? Description);
