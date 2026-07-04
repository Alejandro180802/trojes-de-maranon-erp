using TrojesDeMaranon.Domain.Common;

namespace TrojesDeMaranon.Domain.Security;

public sealed class Permission : AuditableEntity
{
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<RolePermission> RolePermissions { get; set; } = [];
}
