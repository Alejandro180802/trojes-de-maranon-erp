using TrojesDeMaranon.Domain.Common;

namespace TrojesDeMaranon.Domain.Security;

public sealed class Role : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public List<UserRole> UserRoles { get; set; } = [];
    public List<RolePermission> RolePermissions { get; set; } = [];
}
