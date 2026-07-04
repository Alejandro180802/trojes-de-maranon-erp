using TrojesDeMaranon.Domain.Common;
using TrojesDeMaranon.Domain.Companies;

namespace TrojesDeMaranon.Domain.Security;

public sealed class User : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastLoginAt { get; set; }
    public List<UserRole> UserRoles { get; set; } = [];
    public List<RefreshToken> RefreshTokens { get; set; } = [];
}
