using TrojesDeMaranon.Domain.Common;

namespace TrojesDeMaranon.Domain.Security;

public sealed class RefreshToken : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;
}
