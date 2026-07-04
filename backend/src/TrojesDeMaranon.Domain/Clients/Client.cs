using TrojesDeMaranon.Domain.Common;

namespace TrojesDeMaranon.Domain.Clients;

public sealed class Client : AuditableEntity, ICompanyScoped
{
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}
